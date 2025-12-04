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
        private bool Running = false;   // 전체 시스템 동작 플래그

        public FormGraphic formGraphic = new FormGraphic();
        public FormDataChart formDataChart = new FormDataChart();
        #endregion ## Auto Define ##

        private Auto()
        {
            Server = new TcpJsonServer();
            DataManager = new JubbyDataManager();
        }

        #region ## Auto Run ##
        ///  RUN — 전체 시스템 시작
        public void Run()
        {
            if (Running)
                return;

            Running = true;

            /// 1. 서버 시작
            // Python에서 메시지를 보내올 때마다 자동 호출되는 이벤트
            Server.OnMessageReceived += DataManager.HandleMessage;
            // Python이 접속했을 때
            //Server.OnClientConnected += (c) => MainForm.Ins.AddLog("Python Connected");
            // Python 연결이 끊겼을 때
            //Server.OnClientDisconnected += (c) => MainForm.Ins.AddLog("Python Disconnected");

            Server.Start(9001);   // 포트번호 네가 사용하는 값

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

            //MainForm.Ins.AddLog("[AUTO] 자동시스템 시작됨");
        }
        #endregion ## Auto Run ##

        #region ## Auto Stop ##
        //  STOP() — 전체 시스템 종료
        public void Stop()
        {
            if (Running)
                return;

            Running = false;

            // 서버 정지
            Server.Stop();

            //MainForm.Ins.AddLog("[AUTO] 자동시스템 종료됨");
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
