using Jubby_AutoTrade_UI.COM;
using Jubby_AutoTrade_UI.GUI;
using Jubby_AutoTrade_UI.SEQUENCE;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            private readonly object OrderLock = new object();

            public List<TradeOrderData> OrderHistory { get; set; } = new List<TradeOrderData>();

            // 3-3-1. 주문 추가 함수 (Add)
            public void AddOrder(TradeOrderData order)
            {
                // lock: "내가 다 쓸 때까지 아무도 건드리지 마!" (안전장치)
                lock (OrderLock)
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
                lock (OrderLock)
                {
                    // 람다식: "리스트 안에 있는 놈들 중에(x), ID가 orderID랑 똑같은 놈 다 지워라"
                    OrderHistory.RemoveAll(x => x.Order_Type == orderID);
                }
            }

            // 3-3-3. 주문 전체 삭제 (초기화)
            public void ClearOrders()
            {
                lock (OrderLock)
                {
                    OrderHistory.Clear();
                }
            }

            // 3-3-4. 안전하게 리스트 가져오기 (UI에서 그릴 때 복사본 전달)
            public List<TradeOrderData> GetOrderListSafe()
            {
                lock (OrderLock)
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

        #region ## Jubby Data Manager ##
        ///  MESSAGE → JUBBYSTOCKINFO APPLY LOGIC (핵심 부분)
        public class JubbyDataManager
        {
            /// 종목별 저장소
            private Dictionary<string, JubbyStockInfo> _stocks = new Dictionary<string, JubbyStockInfo>();

            /// JSON MESSAGE 처리 → JubbyStockInfo 업데이트
            public void HandleMessage(JsonMessage msg)
            {
                // heartbeat는 무시
                if (msg.MsgType.Equals("heartbeat", StringComparison.OrdinalIgnoreCase))
                    return;

                // msg_type → UpdateTarget 매핑
                if (!Enum.TryParse<UpdateTarget>(msg.MsgType, true, out UpdateTarget target))
                {
                    Console.WriteLine($"[ERROR] Unknown msg_type : {msg.MsgType}");
                    return;
                }

                // symbol 필드 확인
                string symbol = msg.Payload["symbol"]?.ToString();
                if (string.IsNullOrEmpty(symbol))
                {
                    Console.WriteLine("[ERROR] payload.symbol 없음");
                    return;
                }

                // 필수: 종목명(name)
                string name = msg.Payload["name"]?.ToString();
                if (string.IsNullOrEmpty(name))
                {
                    Console.WriteLine("[ERROR] payload.name 없음");
                    return;
                }

                // 종목 찾기 OR 새로 만들기
                if (!_stocks.TryGetValue(symbol, out JubbyStockInfo info))
                {
                    // ★ 여기 수정됨: name 을 반드시 저장
                    info = new JubbyStockInfo(symbol, name);
                    _stocks[symbol] = info;
                }
                else
                {
                    // 이미 존재하더라도 name 변경 가능하도록 갱신해줌 (중요)
                    info.Name = name;
                }
                // 업데이트 적용
                ApplyUpdate(info, target, msg.Payload);

                // UI 테이블 업데이트
                Auto.Ins.formDataChart?.ApplyStockUpdate(info, target);
            }

            // APPLY UPDATE TARGET
            private void ApplyUpdate(JubbyStockInfo info, UpdateTarget target, JToken p)
            {
                switch (target)
                {
                    case UpdateTarget.Market:
                        ApplyMarket(info.Market, p);
                        break;

                    case UpdateTarget.Account:
                        ApplyAccount(info.MyAccount, p);
                        break;

                    case UpdateTarget.OrderHistory:
                        ApplyOrder(info, p);
                        break;

                    case UpdateTarget.Strategy:
                        ApplyStrategy(info.Strategy, p);
                        break;

                    case UpdateTarget.All:
                        if (p["market"] != null) ApplyMarket(info.Market, p["market"]);
                        if (p["account"] != null) ApplyAccount(info.MyAccount, p["account"]);
                        if (p["order"] != null) ApplyOrder(info, p["order"]);
                        if (p["strategy"] != null) ApplyStrategy(info.Strategy, p["strategy"]);
                        break;
                }
            }

            private void ApplyMarket(TradeMarketData m, JToken p)
            {
                m.Last_Price = p["last_price"]?.Value<decimal>() ?? m.Last_Price;
                m.Open_Price = p["open_price"]?.Value<decimal>() ?? m.Open_Price;
                m.High_Price = p["high_price"]?.Value<decimal>() ?? m.High_Price;
                m.Low_Price = p["low_price"]?.Value<decimal>() ?? m.Low_Price;

                m.Bid_Price = p["bid_price"]?.Value<decimal>() ?? m.Bid_Price;
                m.Ask_Price = p["ask_price"]?.Value<decimal>() ?? m.Ask_Price;

                m.Bid_Size = p["bid_size"]?.Value<decimal>() ?? m.Bid_Size;
                m.Ask_Size = p["ask_size"]?.Value<decimal>() ?? m.Ask_Size;

                m.Volume = p["volume"]?.Value<decimal>() ?? m.Volume;
            }

            private void ApplyAccount(TradeAccountData acc, JToken p)
            {
                acc.Quantity = p["quantity"]?.Value<decimal>() ?? acc.Quantity;
                acc.Avg_Price = p["avg_price"]?.Value<decimal>() ?? acc.Avg_Price;
                acc.Pnl = p["pnl"]?.Value<decimal>() ?? acc.Pnl;
                acc.Available_Cash = p["available_cash"]?.Value<decimal>() ?? acc.Available_Cash;
            }

            private void ApplyOrder(JubbyStockInfo info, JToken p)
            {
                TradeOrderData o = new TradeOrderData
                {
                    Order_Type = p["order_type"]?.ToString(),
                    Order_Price = p["order_price"]?.Value<decimal>() ?? 0,
                    Order_Quantity = p["order_quantity"]?.Value<decimal>() ?? 0,
                    Filled_Quqntity = p["filled_quantity"]?.Value<decimal>() ?? 0,
                    Order_Time = p["order_time"]?.ToString(),
                    Status = p["Status"]?.ToString(),
                };

                info.AddOrder(o);
            }

            private void ApplyStrategy(TradeStrategyData s, JToken p)
            {
                s.Ma_5 = p["ma_5"]?.Value<decimal>() ?? s.Ma_5;
                s.Ma_20 = p["ma_20"]?.Value<decimal>() ?? s.Ma_20;
                s.RSI = p["RSI"]?.Value<decimal>() ?? s.RSI;
                s.MACD = p["macd"]?.Value<decimal>() ?? s.MACD;
                s.Signal = p["signal"]?.ToString() ?? s.Signal;
            }

            public string FirstSymbol { get; private set; } = null;
            public bool FirstDataReceived => FirstSymbol != null;

            public JubbyStockInfo GetStock(string symbol)
            {
                if (_stocks == null)
                    return null;

                if (_stocks.TryGetValue(symbol, out JubbyStockInfo info))
                    return info;

                return null;
            }
        }

        #endregion ## Jubby Data Manager ##

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
            public decimal RSI { get; set; }                   // 3. RSI 지표
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
