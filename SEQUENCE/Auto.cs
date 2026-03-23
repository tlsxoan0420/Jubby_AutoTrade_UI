using System;
using System.Collections.Generic;
using System.IO;            // 💡 [추가] 폴더 생성을 위해 추가
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jubby_AutoTrade_UI.COM;
using Jubby_AutoTrade_UI.COMMON;
using Jubby_AutoTrade_UI.GUI;
using static Jubby_AutoTrade_UI.COMMON.Flag;

namespace Jubby_AutoTrade_UI.SEQUENCE
{
    class Auto
    {
        #region ## Class Inatance ##
        private static Auto _ins = new Auto();
        public static Auto Ins => _ins;
        #endregion ## Class Inatance ##

        #region ## Auto Define ##
        private TcpJsonServer Server;
        public JubbyDataManager DataManager;

        private Thread AutoThread;
        public bool Running = false;   // 전체 시스템 동작 플래그

        public FormGraphic formGraphic = new FormGraphic();
        public FormDataChart formDataChart = new FormDataChart();
        #endregion ## Auto Define ##

        private Auto()
        {
            Server = new TcpJsonServer();
            DataManager = new JubbyDataManager();

            // 이벤트 구독은 프로그램 실행 시(생성자) 한 번만 등록
            Server.OnMessageReceived += DataManager.HandleMessage;
            Server.OnClientDisconnected += Server_OnClientDisconnected;
        }

        #region ## Event Handlers ##
        private void Server_OnClientDisconnected(System.Net.Sockets.TcpClient c)
        {
            // 현재 자동매매 모드였다면 강제로 대기 모드로 전환
            if (Flag.Live.Runmode == Flag.ModeNumber.Auto)
            {
                // UI 스레드 충돌 및 프리징을 막기 위해 핸들이 있을 때만 비동기(BeginInvoke)로 실행
                if (formGraphic.IsHandleCreated)
                {
                    formGraphic.BeginInvoke(new Action(() =>
                    {
                        Share.Ins.MessageFormOpen("⚠️ Python 통신이 끊어졌습니다!\n안전을 위해 대기 모드로 전환합니다.");
                        Share.Ins.ChangeMode(Flag.ModeNumber.Home);
                    }));
                }
            }
        }
        #endregion ## Event Handlers ##

        #region ## Auto Run ##
        ///  RUN — 전체 시스템 시작
        public void Run()
        {
            if (Running)
                return;

            Running = true;

            /// 1. 서버 시작 (9001 포트 오픈)
            Server.Start(9001);

            /// 2. 데이터 차트 업데이트 시작
            formDataChart.timer1.Interval = 100;
            formDataChart.timer1.Enabled = true;

            /// 3. 그래프 업데이트 시작
            formGraphic.timer1.Interval = 100;
            formGraphic.timer1.Enabled = true;

            /// 4. Auto 루프 시작
            AutoThread = new Thread(AutoLoop);
            AutoThread.IsBackground = true;
            AutoThread.Start();
        }
        #endregion ## Auto Run ##

        #region ## Auto Stop ##
        //  STOP() — 전체 시스템 종료
        public void Stop()
        {
            if (!Running)
                return;

            Running = false;

            // 1. 서버 정지
            Server.Stop();

            // 💡 [에러 해결] 2. 자동매매가 멈출 때 차트를 백업(저장)하라는 명령 전달
            if (formGraphic != null && formGraphic.IsHandleCreated && !formGraphic.IsDisposed)
            {
                formGraphic.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        // 1. Chart_Backups 라는 폴더가 없으면 만듭니다.
                        string backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Chart_Backups");
                        if (!Directory.Exists(backupFolder))
                        {
                            Directory.CreateDirectory(backupFolder);
                        }

                        // 2. 현재 시간을 넣어서 저장될 파일 이름을 만듭니다. (예: Jubby_Chart_20260323_203015.png)
                        string fileName = $"Jubby_Chart_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        string fullPath = Path.Combine(backupFolder, fileName);

                        // 3. 인수를 채워서 함수를 호출합니다!
                        formGraphic.SaveInteractiveChart(fullPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[자동저장 오류] {ex.Message}");
                    }
                }));
            }
        }
        #endregion ## Auto Stop ##

        #region ## Get Stock ##
        public JubbyStockInfo GetStock(string symbol)
        {
            return DataManager.GetStock(symbol);
        }
        #endregion ## Get Stock ##

        #region ## Auto Loop ##
        /// Auto 루프
        private void AutoLoop()
        {
            while (Running)
            {
                try
                {

                }
                catch
                {

                }

                Thread.Sleep(100); // 0.1초마다 갱신
            }
        }
        #endregion ## Auto Loop ##
    }
}