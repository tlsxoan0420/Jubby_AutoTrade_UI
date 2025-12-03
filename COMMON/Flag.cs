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

            public static bool IsDataChartUpdate = false;

            public static bool IsCommunication = false;
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
        public enum UpdateTarget
        {
            Market,             // 1. 시세만 (Market)
            Account,            // 2. 내 잔고만 (MyAccount)
            OrderHistory,       // 3. 주문내역만 (OrderHistory)
            Strategy,           // 4. 전략 신호만 (Strategy)
            All,                // 5. 전체 (All)
        }

        public class JubbyStockInfo
        {
            public string Symbol { get; set; }          // 1. 종목 코드
            public string Name { get; set; }            // 2. 종목명

            // 3. 구성품 연결 (Composition)
            // 3-1. 시세 정보 (항상 존재함)
            public TradeMarketData Market { get; set; } = new TradeMarketData();

            // 3-2. 내 잔고 정보 (내가 샀을 때만 데이터가 있음 -> 없으면 null일 수도 있음)
            public TradeAccountData MyAccount { get; set; } = new TradeAccountData();

            // 3-3. 주문내역([중요]'여러 개'일 수 있으므로 List로 관리해야 함)

            // [핵심] 리스트 보호를 위한 자물쇠 객체 (스레드 충돌 방지)
            private readonly object _orderLock = new object();

            public List<TradeOrderData> OrderHistory { get; set; } = new List<TradeOrderData>();

            // 3-3-1. 주문 추가 함수 (Add)
            public void AddOrder(TradeOrderData order)
            {
                // lock: "내가 다 쓸 때까지 아무도 건드리지 마!" (안전장치)
                lock (_orderLock)
                {
                    OrderHistory.Add(order);

                    // (선택) 데이터가 너무 많이 쌓이면 오래된 것 삭제 (예: 최근 100개만 유지)
                    if (OrderHistory.Count > 100)
                    {
                        OrderHistory.RemoveAt(0); // 맨 앞(가장 오래된 것) 삭제
                    }
                }
            }

            // 3-3-2. 주문 삭제 함수 (Remove) - 주문번호(ID)로 삭제
            public void RemoveOrder(string orderID)
            {
                lock (_orderLock)
                {
                    // 람다식: "리스트 안에 있는 놈들 중에(x), ID가 orderID랑 똑같은 놈 다 지워라"
                    OrderHistory.RemoveAll(x => x.Order_Type == orderID);
                }
            }

            // 3-3-3. 주문 전체 삭제 (초기화)
            public void ClearOrders()
            {
                lock (_orderLock)
                {
                    OrderHistory.Clear();
                }
            }

            // 3-3-4. 안전하게 리스트 가져오기 (UI에서 그릴 때 복사본 전달)
            public List<TradeOrderData> GetOrderListSafe()
            {
                lock (_orderLock)
                {
                    // 원본을 주면 충돌나니까, 똑같은 리스트를 복사해서 줌 (ToList)
                    return OrderHistory.ToList();
                }
            }

            // 3-4. 전략 분석 정보 (항상 계산됨)
            public TradeStrategyData Strategy { get; set; } = new TradeStrategyData();

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
            public decimal Last_Price { get; set; }        // 1. 현재가
            public decimal Open_Price { get; set; }        // 2. 시가
            public decimal High_Price { get; set; }        // 3. 고가
            public decimal Low_Price { get; set; }         // 4. 저가
            public decimal Bid_Price { get; set; }         // 5. 매수호가
            public decimal Ask_Price { get; set; }         // 6. 매도호가
            public decimal Bid_Size { get; set; }          // 7. 매수잔량
            public decimal Ask_Size { get; set; }          // 8. 매도잔량
            public decimal Volume { get; set; }            // 9. 거래량
        }

        // 2. 잔고 정보 데이터
        public class TradeAccountData
        {
            public decimal Quantity { get; set; }          // 1. 보유수량
            public decimal Avg_Price { get; set; }         // 2. 평균 매입가
            public decimal Pnl { get; set; }               // 3. 평가손익
            public decimal Available_Cash { get; set; }    // 4. 주문 가능 금액
        }

        // 3. 전략 분석 정보 데이터
        public class TradeOrderData
        {
            public string Order_Type { get; set; }       // 1. 주문종류
            public decimal Order_Price { get; set; }       // 2. 주문가격
            public decimal Order_Quantity { get; set; }    // 3. 주문수량
            public decimal Filled_Quqntity { get; set; }   // 4. 체결수량
            public string Order_Time { get; set; }        // 5. 주문시간
            public string Status { get; set; }            // 6. 주문상태
        }

        // 4. 주문 내역 데이터
        public class TradeStrategyData
        {
            public decimal Ma_5 { get; set; }                  // 1. 단기 이동평균
            public decimal Ma_20 { get; set; }                 // 2. 장기 이동평균
            public decimal RIS { get; set; }                   // 3. RSI 지표
            public decimal MACD { get; set; }                  // 4. MACD 지표
            public string Signal { get; set; }                // 5. 전략 신호 (매수 / 매도 / NONE)
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
