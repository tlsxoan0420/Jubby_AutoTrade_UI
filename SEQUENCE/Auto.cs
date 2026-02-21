using Jubby_AutoTrade_UI.COM;
using Jubby_AutoTrade_UI.COMMON;
using Jubby_AutoTrade_UI.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

            // [수정 1] 이벤트 구독은 프로그램 실행 시(생성자) 한 번만 등록하도록 이동
            // Run() 안에 있으면 실행할 때마다 누적 등록되어 에러창이 여러 개 뜨는 버그가 발생함.
            Server.OnMessageReceived += DataManager.HandleMessage;
            Server.OnClientDisconnected += Server_OnClientDisconnected;
        }

        #region ## Event Handlers ##
        // [수정 2] 통신 끊김 이벤트 로직 분리 및 UI 데드락 방지를 위한 BeginInvoke 사용
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
            // [수정 3] 치명적 오타 수정: Running이 '아닐 때(!)' 리턴해야 함. (기존 if(Running)은 서버를 못 끄게 만듦)
            if (!Running)
                return;

            Running = false;

            // 서버 정지
            Server.Stop();
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