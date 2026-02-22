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

        #region ## 서버 시작 ##
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
        #endregion ## 서버 시작 ##

        #region ## 서벗 종료 ##
        // 서버 종료
        public void Stop()
        {
            Running = false;
            Listener?.Stop();
        }
        #endregion ## 서버 종료 ##

        #region ## 접속 대기 스레드 ##
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
        #endregion ## 접속 대기 스레드 ##

        #region ## 클라이언트 데이터 수신 ##
        /// 실제로 데이터를 주고받는 루프 (클라이언트 1개당 1개 스레드)
        // Python이 보내는 모든 메시지를 이곳에서 처리
        private void HandleClient(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            DateTime lastMessageTime = DateTime.UtcNow;

            try
            {
                while (client.Connected)
                {
                    // 수정된 부분: 데이터가 아직 안 온 상태에서 타임아웃 체크 (무한 대기 방지)
                    if (!ns.DataAvailable)
                    {
                        if (HeartbeatTimeoutSeconds > 0 &&
                            (DateTime.UtcNow - lastMessageTime).TotalSeconds > HeartbeatTimeoutSeconds)
                        {
                            Console.WriteLine("Heartbeat Timeout: 연결이 끊어진 것으로 간주합니다.");
                            break;
                        }
                        Thread.Sleep(10); // CPU 점유율 방지를 위해 짧게 대기
                        continue;
                    }

                    // 6바이트 헤더 읽기
                    byte[] header = new byte[6];
                    if (!ReadExact(ns, header, 0, 6))
                        break;

                    byte version = header[0];
                    byte flags = header[1];
                    bool compressed = (flags & 0x01) != 0;
                    int len = (header[2] << 24) | (header[3] << 16) | (header[4] << 8) | (header[5]);

                    if (len <= 0) continue;

                    byte[] body = new byte[len];
                    if (!ReadExact(ns, body, 0, len))
                        break;

                    if (compressed)
                        body = Decompress(body);

                    string jsonText = Encoding.UTF8.GetString(body);
                    JObject obj = JObject.Parse(jsonText);
                    lastMessageTime = DateTime.UtcNow;

                    var msg = new JsonMessage
                    {
                        MsgType = obj["msg_type"]?.ToString() ?? "",
                        Timestamp = obj["timestamp"] != null ? DateTime.Parse(obj["timestamp"].ToString()) : DateTime.UtcNow,
                        Payload = obj["payload"]
                    };

                    // 수정된 부분: 메시지 처리 에러가 소켓을 끊지 못하도록 분리
                    try
                    {
                        if (OnMessageReceived != null)
                            OnMessageReceived(msg);
                    }
                    catch (Exception innerEx)
                    {
                        // 데이터 처리에 실패해도 통신은 살려둡니다.
                        Console.WriteLine("메시지 데이터 처리 중 오류 발생 (연결은 유지됨): " + innerEx.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HandleClient Exception: " + ex.Message);
            }
            finally
            {
                if (OnClientDisconnected != null)
                    OnClientDisconnected(client);

                client.Close();
            }
        }
        #endregion ## 클라이언트 데이터 수신 ##

        #region ## TCP 데이터 안정 수신 ##
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
        #endregion ## TCP 데이터 안정 수신 ##

        #region ## 데이터 압축 해제 ##
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
        #endregion ## 데이터 압축 해제 ##
    }

    #region ## JSON 메시지 모델 ##
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
    #endregion ## JSON 메시지 모델 ##
}
