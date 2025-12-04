using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Jubby_AutoTrade_UI.COM
{
    /// Python → C# 실시간 데이터 스트림을 받기 위한 TCP 서버
    // 패킷 프로토콜: [1byte version][1byte flags][4byte bodyLength][body]
    // body는 JSON 또는 압축된 JSON
    // 대량 데이터 안정적으로 수신 가능
    // Heartbeat 기반으로 클라이언트 생존 여부 체크 가능

    class TcpJsonServer
    {
        private TcpListener Listener;  // TCP 서버 리스너
        private Thread ListenThread;  // 클라이언트 접속 감지 스레드
        private bool Running = false;  // 서버 실행 여부

        public int Port { get; private set; }  // 서버 포트 번호

        /// Python에서 메시지를 보내올 때마다 자동 호출되는 이벤트
        public event Action<JsonMessage> OnMessageReceived;

        /// Python이 접속했을 때
        public event Action<TcpClient> OnClientConnected;

        /// Python 연결이 끊겼을 때
        public event Action<TcpClient> OnClientDisconnected;

        /// Heartbeat 타임아웃 시간(초)
        // Python이 heartbeat 메시지를 일정 시간 동안 보내지 않으면 연결 끊김으로 판단함
        // 0이면 사용 안 함
        public int HeartbeatTimeoutSeconds { get; set; } = 50;


        /// 서버 시작
        public void Start(int port = 9001)
        {
            Port = port;

            // 모든 IP에서 접속 허용
            Listener = new TcpListener(IPAddress.Any, port);
            Listener.Start();
            Running = true;

            // 클라이언트 접속 감지 스레드 시작
            ListenThread = new Thread(ListenLoop);
            ListenThread.IsBackground = true;
            ListenThread.Start();
        }


        // 서버 종료
        public void Stop()
        {
            Running = false;
            Listener?.Stop();
        }


        /// Python 클라이언트가 접속할 때까지 계속 대기하는 스레드
        private void ListenLoop()
        {
            while (Running)
            {
                try
                {
                    if (Listener == null)
                        break;

                    // Python에서 접속하면 여기서 반환
                    TcpClient client = Listener.AcceptTcpClient();

                    // 접속한 쪽 IP 확인 (디버깅용)
                    string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    Console.WriteLine("Client Connected: " + clientIP);

                    // 접속 이벤트 호출
                    if (OnClientConnected != null)
                        OnClientConnected(client);

                    // 이 클라이언트 전용으로 수신 담당 스레드 하나 생성
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
                catch
                {
                    if (!Running) break;
                }
            }
        }


        /// 실제로 데이터를 주고받는 루프 (클라이언트 1개당 1개 스레드)
        // Python이 보내는 모든 메시지를 이곳에서 처리
        private void HandleClient(TcpClient client)
        {
            NetworkStream ns = client.GetStream();

            // 마지막 메시지를 받은 시간 → Heartbeat 감지용
            DateTime lastMessageTime = DateTime.UtcNow;

            try
            {
                while (client.Connected)
                {
                    // Heartbeat 타임아웃 체크
                    if (HeartbeatTimeoutSeconds > 0 &&
                        (DateTime.UtcNow - lastMessageTime).TotalSeconds > HeartbeatTimeoutSeconds)
                    {
                        // 일정 시간 동안 아무 메시지도 안 왔으면 끊어진 걸로 처리
                        break;
                    }

                    // 6바이트 헤더 읽기
                    byte[] header = new byte[6];

                    // ReadExact는 요청한 크기만큼 정확히 수신하는 함수 (중요!)
                    if (!ReadExact(ns, header, 0, 6))
                        break;

                    // 프로토콜 정보 읽기
                    byte version = header[0];         // 현재 버전=1
                    byte flags = header[1];           // 압축 여부
                    bool compressed = (flags & 0x01) != 0;

                    // body 길이 (big-endian)
                    int len = (header[2] << 24) |
                              (header[3] << 16) |
                              (header[4] << 8) |
                              (header[5]);

                    if (len <= 0)
                        continue;   // 데이터 없음

                    // body 읽기
                    byte[] body = new byte[len];
                    if (!ReadExact(ns, body, 0, len))
                        break;

                    // 압축 데이터면 해제
                    if (compressed)
                        body = Decompress(body);

                    // UTF-8 JSON 문자열로 변환
                    string jsonText = Encoding.UTF8.GetString(body);

                    // JSON 파싱
                    JObject obj = JObject.Parse(jsonText);

                    lastMessageTime = DateTime.UtcNow;  // heartbeat 갱신

                    // JsonMessage 객체로 변환
                    var msg = new JsonMessage
                    {
                        MsgType = obj["msg_type"]?.ToString() ?? "",
                        Timestamp = obj["timestamp"] != null
                            ? DateTime.Parse(obj["timestamp"].ToString())
                            : DateTime.UtcNow,
                        Payload = obj["payload"]
                    };

                    // UI 또는 외부 코드로 메시지 전달
                    if (OnMessageReceived != null)
                        OnMessageReceived(msg);
                }
            }
            catch(Exception ex)
            {
                // 통신 중 오류 발생
                Console.WriteLine("HandleClient Exception: " + ex.Message);
            }
            finally
            {
                // 접속 끊김 처리
                if (OnClientDisconnected != null)
                    OnClientDisconnected(client);

                client.Close();
            }
        }


        /// TCP Stream이 요청한 데이터 크기만큼 정확히 읽어올 때까지 반복
        // TCP는 패킷을 한 번에 안 줌
        // 그래서 대량 데이터 안정 수신에 필수
        private bool ReadExact(NetworkStream ns, byte[] buffer, int offset, int size)
        {
            int readTotal = 0;

            while (readTotal < size)
            {
                int read = ns.Read(buffer, offset + readTotal, size - readTotal);

                // 0 또는 음수 = 연결 끊김
                if (read <= 0)
                    return false;

                readTotal += read;
            }
            return true;
        }


        /// Python에서 zlib 압축된 데이터를 보냈다면 여기서 해제
        private byte[] Decompress(byte[] data)
        {
            using (var input = new MemoryStream(data))
            using (var ds = new DeflateStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                ds.CopyTo(output);
                return output.ToArray();
            }
        }
    }


    /// JSON 메시지를 C# 객체로 변환한 모델
    // Msg_Type : Market / Account / OrderHistory / Strategy / All / heartbeat...
    // Timestamp: 데이터 생성 시간
    // Payload: 실제 데이터(가격·주문 등)
    public class JsonMessage
    {
        public string MsgType { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public JToken Payload { get; set; }
    }
}
