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
            public static int OldFormMode = 0;

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
            Order,       // 3. 주문내역만 (OrderHistory)
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
        ///  MESSAGE → JUBBYSTOCKINFO APPLY LOGIC (핵심 컨트롤 타워)
        public class JubbyDataManager
        {
            /// 📚 종목별 보관소 (C#이 켜져 있는 동안 삼성전자, SK하이닉스 등의 데이터를 기억하는 장부입니다)
            private Dictionary<string, JubbyStockInfo> _stocks = new Dictionary<string, JubbyStockInfo>();

            /// 📡 파이썬 택배(JSON)가 도착하면 실행되는 함수
            public void HandleMessage(JsonMessage msg)
            {
                // 1. 단순 연결 확인(heartbeat) 신호는 그릴 게 없으니 가볍게 패스합니다.
                if (msg.MsgType.Equals("heartbeat", StringComparison.OrdinalIgnoreCase))
                    return;

                // 2. 이 데이터가 마켓 정보인지, 계좌 정보인지 이름표(MsgType)를 확인합니다.
                if (!Enum.TryParse<UpdateTarget>(msg.MsgType, true, out UpdateTarget target))
                {
                    Console.WriteLine($"[ERROR] 알 수 없는 데이터 타입 : {msg.MsgType}");
                    return;
                }

                // 🚨 [데이터 뭉침 해결의 열쇠] 🚨
                // 파이썬이 이제 종목 10개를 묶어서(배열 형태) 보냅니다.
                if (msg.Payload is JArray payloadArray)
                {
                    // 💡 배열 안에 든 10개의 종목을 하나씩 꺼내서(foreach) 처리합니다.
                    foreach (JToken item in payloadArray)
                    {
                        // 1. 종목코드(symbol)를 가져옵니다. (없으면 불량품이니 다음 걸로!)
                        string symbol = item["symbol"]?.ToString();
                        if (string.IsNullOrEmpty(symbol)) continue;

                        // 2. 회사이름(symbol_name)을 가져옵니다. (삼성전자, SK하이닉스 등)
                        string name = item["symbol_name"]?.ToString() ?? symbol;

                        // 💡 "내 장부에 이 종목이 처음인가?" 확인하고 없으면 새로 만듭니다.
                        if (!_stocks.TryGetValue(symbol, out JubbyStockInfo info))
                        {
                            info = new JubbyStockInfo(symbol, name);
                            _stocks[symbol] = info;

                            // 장부의 맨 첫 번째 종목은 '대표 종목'으로 기억해둡니다 (화면 초기 세팅용)
                            if (FirstSymbol == null) FirstSymbol = symbol;
                        }
                        else
                        {
                            // 이미 있는 종목이면 이름만 혹시 모르니 다시 적어줍니다.
                            info.Name = name;
                        }

                        // 💡 [에러 해결 포인트] 파이썬의 '글자' 데이터를 C#의 '숫자'로 안전하게 변환해서 저장합니다.
                        ApplyUpdate(info, target, item);

                        // 3. UI 표(DataGridView)에 데이터를 갱신하라고 명령합니다.
                        // 💡 여기서 서로 다른 종목코드(000, 001...)를 쓰기 때문에 이제 표에 10줄이 쫙 나옵니다!
                        Auto.Ins.formDataChart?.SafeApplyStockUpdate(info, target);

                        // 4. 차트 화면(FormGraphic)에도 데이터를 던져서 실시간 캔들을 그리게 합니다.
                        if (target == UpdateTarget.Market || target == UpdateTarget.All)
                        {
                            // 🚀 [중요] 차트는 데이터 하나하나가 중요하므로 Invoke로 순서대로 밀어넣습니다.
                            Auto.Ins.formGraphic?.Invoke(new Action(() => {
                                Auto.Ins.formGraphic.UpdateMarketData(info);
                            }));
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"[ERROR] 데이터 형식이 배열(List)이 아닙니다! MsgType: {msg.MsgType}");
                }
            }

            // ====================================================================
            // ✨ [만능 숫자 변환기: ParseDecimal]
            // 파이썬은 "3.50%"(퍼센트) 나 "5,000,000"(쉼표) 처럼 사람이 보기 편하게 '글자'로 보냅니다.
            // C#은 '숫자 방(decimal)'에 글자를 넣으면 에러가 납니다.
            // 그래서 기호(%, ,)를 싹 지우고 순수한 숫자로 바꿔주는 세탁기 함수를 만들었습니다.
            // ====================================================================
            private decimal ParseDecimal(JToken token, decimal defaultValue)
            {
                if (token == null) return defaultValue; // 값이 비어있으면 원래 값을 그대로 씁니다.

                // 1. 글자에서 쉼표(,)와 퍼센트(%)를 지우고 빈칸을 제거합니다.
                string strVal = token.ToString().Replace(",", "").Replace("%", "").Trim();

                // 2. 숫자로 변환에 성공하면 그 값을 돌려주고, 실패하면 원래 값을 돌려줍니다.
                if (decimal.TryParse(strVal, out decimal result))
                {
                    return result;
                }
                return defaultValue;
            }

            // ====================================================================
            // 📝 [데이터 분배 함수] 택배 내용물을 각 주머니(Market, Account 등)에 넣습니다.
            // ====================================================================
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
                    case UpdateTarget.Order:
                        ApplyOrder(info, p);
                        break;
                    case UpdateTarget.Strategy:
                        ApplyStrategy(info.Strategy, p);
                        break;
                    case UpdateTarget.All:
                        // 'All' 타입일 경우 모든 항목을 다 챙깁니다.
                        if (p["market"] != null) ApplyMarket(info.Market, p["market"]);
                        if (p["account"] != null) ApplyAccount(info.MyAccount, p["account"]);
                        if (p["order"] != null) ApplyOrder(info, p["order"]);
                        if (p["strategy"] != null) ApplyStrategy(info.Strategy, p["strategy"]);
                        break;
                }
            }

            private void ApplyMarket(TradeMarketData m, JToken p)
            {
                // 모든 가격 데이터는 세탁기(ParseDecimal)를 거쳐서 숫자로 안전하게 들어갑니다.
                m.Last_Price = ParseDecimal(p["last_price"], m.Last_Price);
                m.Open_Price = ParseDecimal(p["open_price"], m.Open_Price);
                m.High_Price = ParseDecimal(p["high_price"], m.High_Price);
                m.Low_Price = ParseDecimal(p["low_price"], m.Low_Price);
                m.Bid_Price = ParseDecimal(p["bid_price"], m.Bid_Price);
                m.Ask_Price = ParseDecimal(p["ask_price"], m.Ask_Price);
                m.Bid_Size = ParseDecimal(p["bid_size"], m.Bid_Size);
                m.Ask_Size = ParseDecimal(p["ask_size"], m.Ask_Size);
                m.Volume = ParseDecimal(p["volume"], m.Volume);
            }

            private void ApplyAccount(TradeAccountData acc, JToken p)
            {
                acc.Quantity = ParseDecimal(p["quantity"], acc.Quantity);
                acc.Avg_Price = ParseDecimal(p["avg_price"], acc.Avg_Price);

                // 💡 [에러 해결] 파이썬이 보낸 "3.50%" 글자를 숫자로 세탁해서 넣습니다.
                acc.Pnl = ParseDecimal(p["pnl"], acc.Pnl);

                // 💡 [에러 해결] 파이썬이 보낸 "5,000,000" 글자를 숫자로 세탁해서 넣습니다.
                acc.Available_Cash = ParseDecimal(p["available_cash"], acc.Available_Cash);
            }

            private void ApplyOrder(JubbyStockInfo info, JToken p)
            {
                TradeOrderData o = new TradeOrderData
                {
                    Order_Type = p["order_type"]?.ToString(),
                    Order_Price = ParseDecimal(p["order_price"], 0),
                    Order_Quantity = ParseDecimal(p["order_quantity"], 0),
                    Filled_Quqntity = ParseDecimal(p["filled_quantity"], 0),
                    Order_Time = p["order_time"]?.ToString(),
                    Status = p["order_status"]?.ToString(),
                };
                info.AddOrder(o);
            }

            private void ApplyStrategy(TradeStrategyData s, JToken p)
            {
                s.Ma_5 = ParseDecimal(p["ma_5"], s.Ma_5);
                s.Ma_20 = ParseDecimal(p["ma_20"], s.Ma_20);
                s.RSI = ParseDecimal(p["rsi"], s.RSI);

                // 💡 [에러 해결] AI 확률 문자열("75.4%")을 숫자로 변환합니다.
                s.MACD = ParseDecimal(p["macd"], s.MACD);

                s.Signal = p["signal"]?.ToString() ?? s.Signal;
            }

            public string FirstSymbol { get; private set; } = null;
            public bool FirstDataReceived => FirstSymbol != null;

            public JubbyStockInfo GetStock(string symbol)
            {
                if (_stocks == null) return null;
                if (_stocks.TryGetValue(symbol, out JubbyStockInfo info)) return info;
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
            public const int Hide = 5;
            public const int Home = 10;
            public const int Simul = 20;
            public const int Auto = 30;
            public const int Error = 40;
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
    }
}
