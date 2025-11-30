using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jubby_AutoTrade_UI.GUI;

namespace Jubby_AutoTrade_UI.COMMON
{
    class Flag
    {
        #region ## Define ##
        public class Define
        {
            public const string APP_NAME = "Jubby AutoTrade UI";
            public const string APP_VERSION = "v1.0.0";

            public bool IsSimulation = false;
            public bool IsAuto = false;
        }
        #endregion ## Define ##

        #region ## Live ##
        public class Live
        {
            public static int Runmode = 0;

            public static int iReadyTime = 0;
            public static int iReadyPuaseTime = 0;
            public static int iSimulOperationTime = 0;
            public static int iSimulOperationPuaseTime = 0;
            public static int iAutoOperationTime = 0;
            public static int iAutoOperationPuaseTime = 0;
            public static int iErrorTime = 0;

            public static bool IsLogin = false;
            public static bool FormChange = false;
            public static bool IsMessageOkClick = false;
            public static bool ErrorClearSuccess = false;
        }
        #endregion ## Live ##

        #region ## User Status##
        public class UserStatus
        {
            static public int Level;

            static public string Password;
            static public string Name;
            static public string LoginID;
        }
        #endregion ## User Status##

        #region ## Trade Data ##

        // 1. 계좌 / 표지션 데이터
        public class TradeMarketData
        {
            static public double Last_Price = 0;        // 1. 현재가
            static public double Open_Price = 0;        // 2. 시가
            static public double High_Price = 0;        // 3. 고가
            static public double Low_Price = 0;         // 4. 저가
            static public double Bid_Price = 0;         // 5. 매수호가
            static public double Ask_Price = 0;         // 6. 매도호가
            static public double Bid_Size = 0;          // 7. 매수잔량
            static public double Ask_Size = 0;          // 8. 매도잔량
            static public double Volume = 0;            // 9. 거래량
        }

        // 2. 시장 호가 데이터
        public class TradeAccountData
        {
            static public string Symbol = "";           // 1. 종목코드
            static public double Quantity = 0;          // 2. 보유수량
            static public double Avg_Price = 0;         // 3. 평균 매입가
            static public double Pnl = 0;               // 4. 평가손익
            static public double Available_Cash = 0;    // 5. 주문 가능 금액
        }

        // 3. 주문 상태 데이터
        public class TradeOrderData
        {
            static public double Order_ID = 0;          // 1. 주문번호
            static public string Order_Trype = "";      // 2. 주문종류
            static public double Order_Price = 0;       // 3. 주문가격
            static public double Order_Quantity = 0;    // 4. 주문수량
            static public double Filled_Quqntity = 0;   // 5. 체결수량
            static public string Order_Time = "";       // 6. 주문시간
            static public string Status = "";           // 7. 주문상태
        }

        public class TradeStrategyData
        {
            static public string Symbol = "";                // 1. 종목
            static public double Ma_5 = 0;                  // 2. 단기 이동평균
            static public double Ma_20 = 0;                 // 3. 장기 이동평균
            static public double RIS = 0;                   // 4. RSI 지표
            static public double MACD = 0;                  // 5. MACD 지표
            static public string Signal = "";               // 6. 전략 신호 (매수 / 매도 / NONE)
        }
        #endregion ## Trade Data ##

        #region ## Mode Number ##
        public class ModeNumber
        {
            public const int Logout = 0;
            public const int Home = 1;
            public const int Simul = 2;
            public const int Auto = 3;
            public const int Error = 4;
        }
        #endregion ## Mode Number ##

        #region ## User Level ##
        public class UserLevel
        {
            public const int GUEST = 0;
            public const int ADMIN = 1;
            public const int MASTER = 2;
        }
        #endregion ## User Level ##

        #region ## Page Number ##
        public class PageNumber
        {
            public const int FORM_LOGOUT = 0;
            public const int FORM_HOME = 1;
            public const int FORM_AUTO = 2;
            public const int FORM_ERROR = 3;
        }
        #endregion ## Page Number ##
    }
}
