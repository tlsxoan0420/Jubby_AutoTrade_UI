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

        #region ## Jubby Stock Info ##
        public class JubbyStockInfo
        {
            public string Symbol { get; set; }          // 1. 종목 코드
            public string Name { get; set; }            // 2. 종목명

            // 3. 구성품 연결 (Composition)
            // 3-1. 시세 정보 (항상 존재함)
            public TradeMarketData Market { get; set; } = new TradeMarketData();

            // 3-2. 내 잔고 정보 (내가 샀을 때만 데이터가 있음 -> 없으면 null일 수도 있음)
            public TradeAccountData MyAccount { get; set; } = new TradeAccountData();

            // 3-3. 전략 분석 정보 (항상 계산됨)
            public TradeStrategyData Strategy { get; set; } = new TradeStrategyData();

            // 3-4. 주문내역([중요]'여러 개'일 수 있으므로 List로 관리해야 함)
            public List<TradeOrderData> OrderHistory { get; set; } = new List<TradeOrderData>();

            // 4. 생성자: 종목 코드를 받아서 생성
            public JubbyStockInfo(string symbol, string name)
            {
                this.Symbol = symbol;
                this.Name = name;
            }
        }
        #endregion ## Jubby Stock Info ##

        #region ## Trade Data ##

        // 1.  시세 정보 데이터
        public class TradeMarketData
        {
            public double Last_Price { get; set; }        // 1. 현재가
            public double Open_Price { get; set; }        // 2. 시가
            public double High_Price { get; set; }        // 3. 고가
            public double Low_Price { get; set; }         // 4. 저가
            public double Bid_Price { get; set; }         // 5. 매수호가
            public double Ask_Price { get; set; }         // 6. 매도호가
            public double Bid_Size { get; set; }          // 7. 매수잔량
            public double Ask_Size { get; set; }          // 8. 매도잔량
            public double Volume { get; set; }            // 9. 거래량
        }

        // 2. 잔고 정보 데이터
        public class TradeAccountData
        {
            public double Quantity { get; set; }          // 1. 보유수량
            public double Avg_Price { get; set; }         // 2. 평균 매입가
            public double Pnl { get; set; }               // 3. 평가손익
            public double Available_Cash { get; set; }    // 4. 주문 가능 금액
        }

        // 3. 전략 분석 정보 데이터
        public class TradeOrderData
        {
            static public string Order_Trype { get; set; }       // 1. 주문종류
            static public double Order_Price { get; set; }       // 2. 주문가격
            static public double Order_Quantity { get; set; }    // 3. 주문수량
            static public double Filled_Quqntity { get; set; }   // 4. 체결수량
            static public string Order_Time { get; set; }        // 5. 주문시간
            static public string Status { get; set; }            // 6. 주문상태
        }

        // 4. 주문 내역 데이터
        public class TradeStrategyData
        {
            static public double Ma_5 { get; set; }                  // 1. 단기 이동평균
            static public double Ma_20 { get; set; }                 // 2. 장기 이동평균
            static public double RIS { get; set; }                   // 3. RSI 지표
            static public double MACD { get; set; }                  // 4. MACD 지표
            static public string Signal { get; set; }                // 5. 전략 신호 (매수 / 매도 / NONE)
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
