using Jubby_AutoTrade_UI.COM;
using Jubby_AutoTrade_UI.GUI;
using Jubby_AutoTrade_UI.SEQUENCE;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jubby_AutoTrade_UI.COMMON
{
    public class Flag
    {
        #region ## Define (프로그램 기본 정보) ##
        /// <summary>
        /// 프로그램의 이름, 버전 및 전체적인 동작 모드를 정의하는 클래스입니다.
        /// </summary>
        public class Define
        {
            public const string APP_NAME = "Jubby AutoTrade UI"; // 프로그램 이름
            public const string APP_VERSION = "v1.0.0";          // 프로그램 버전
            public bool IsSimulation = false;                    // 현재 가상(모의)투자 모드인지 여부
            public bool IsAuto = false;                          // 현재 자동매매가 실행 중인지 여부
        }
        #endregion

        #region ## Live (실시간 상태 관리) ##
        /// <summary>
        /// 프로그램이 돌아가는 동안 실시간으로 변하는 상태값(타이머, 화면 상태 등)을 저장합니다.
        /// </summary>
        public class Live
        {
            public static int Runmode = 0;                  // 현재 프로그램 실행 모드 (Home, Simul, Auto 등)
            public static int iReadyTime = 0;               // 대기 시간 카운트
            public static int iReadyPuaseTime = 0;          // 일시정지 시간 카운트
            public static int iSimulOperationTime = 0;      // 시뮬레이션 동작 시간
            public static int iSimulOperationPuaseTime = 0; // 시뮬레이션 일시정지 시간
            public static int iAutoOperationTime = 0;       // 자동매매 동작 시간
            public static int iAutoOperationPuaseTime = 0;  // 자동매매 일시정지 시간
            public static int iErrorTime = 0;               // 에러 발생 후 경과 시간
            public static int OldFormMode = 0;              // 이전 화면 모드 (화면 전환 시 복귀용)
            public static bool IsLogin = false;             // 로그인 성공 여부
            public static bool FormChange = false;          // 화면 전환이 진행 중인지 여부
            public static bool IsMessageOkClick = false;    // 메시지창에서 OK 버튼을 눌렀는지 여부
            public static bool ErrorClearSuccess = false;   // 에러 상황이 성공적으로 해제되었는지 여부
            public static bool IsDataChartUpdate = false;   // 차트/데이터가 업데이트 중인지 여부
            public static bool IsCommunication = false;     // 서버와 통신이 원활한지 여부
        }
        #endregion

        #region ## User Status (사용자 정보) ##
        /// <summary>
        /// 로그인한 사용자의 권한 및 계정 정보를 담고 있습니다.
        /// </summary>
        public class UserStatus
        {
            static public int Level;        // 사용자 권한 레벨 (Guest, Admin, Master)
            static public string Password;  // 사용자 비밀번호
            static public string Name;      // 사용자 이름(닉네임)
            static public string LoginID;   // 사용자 로그인 아이디
        }
        #endregion

        #region ## Jubby Stock Info (주삐 개별 종목 정보 구조체) ##

        /// <summary>
        /// 파이썬에서 업데이트 데이터가 날아올 때, 어떤 부분(시장가, 계좌, 주문, 전략 등)을 갱신할지 결정하는 타겟입니다.
        /// </summary>
        public enum UpdateTarget
        {
            Market, Account, Order, Strategy, All
        }

        /// <summary>
        /// 주식 한 '종목'이 가지는 모든 정보(시장가, 내 계좌 상태, 주문 내역, AI 전략)를 하나의 객체로 묶어둔 클래스입니다.
        /// </summary>
        public class JubbyStockInfo
        {
            public string Symbol { get; set; } // 종목 코드 (예: "005930")
            public string Name { get; set; }   // 종목 이름 (예: "삼성전자")

            public TradeMarketData Market { get; set; } = new TradeMarketData();       // 1. 현재 주식 시장 데이터
            public TradeAccountData MyAccount { get; set; } = new TradeAccountData();  // 2. 내 계좌에 보유 중인 수량 및 수익률

            // 🚨 주문 기록은 여러 스레드에서 동시에 접근할 수 있으므로 Lock을 걸어 안전하게 관리합니다.
            private readonly object OrderLock = new object();
            public List<TradeOrderData> OrderHistory { get; set; } = new List<TradeOrderData>(); // 3. 주문 히스토리

            // 새로운 주문 내역을 리스트에 추가합니다. (메모리 관리를 위해 100개가 넘으면 오래된 것부터 지웁니다)
            public void AddOrder(TradeOrderData order)
            {
                lock (OrderLock)
                {
                    OrderHistory.Add(order);
                    if (OrderHistory.Count > 100) OrderHistory.RemoveAt(0);
                }
            }

            // 특정 주문 번호(Type)를 찾아 삭제합니다.
            public void RemoveOrder(string orderID)
            {
                lock (OrderLock) { OrderHistory.RemoveAll(x => x.Order_Type == orderID); }
            }

            // 주문 내역을 초기화합니다.
            public void ClearOrders()
            {
                lock (OrderLock) { OrderHistory.Clear(); }
            }

            // 외부에서 주문 내역을 안전하게 가져갈 수 있도록 리스트를 복사해서 넘겨줍니다.
            public List<TradeOrderData> GetOrderListSafe()
            {
                lock (OrderLock) { return OrderHistory.ToList(); }
            }

            public TradeStrategyData Strategy { get; set; } = new TradeStrategyData(); // 4. 파이썬이 계산해준 AI 전략 지표값

            // [생성자] 처음 종목 객체를 만들 때 코드와 이름을 부여합니다.
            public JubbyStockInfo(string symbol, string name)
            {
                this.Symbol = symbol;
                this.Name = name;
            }
        }
        #endregion

        #region ## Jubby Data Manager (데이터 총괄 매니저) ##
        /// <summary>
        /// 파이썬 소켓 서버로부터 날아오는 JSON 데이터를 해석하여 각 종목 객체(JubbyStockInfo)에 분배하는 핵심 클래스입니다.
        /// </summary>
        public class JubbyDataManager
        {
            // 수집된 모든 종목들을 '종목코드'를 열쇠(Key)로 삼아 딕셔너리에 보관합니다.
            private readonly Dictionary<string, JubbyStockInfo> _stocks = new Dictionary<string, JubbyStockInfo>();

            public string FirstSymbol { get; private set; } = null;     // 가장 처음 수신된 종목코드 (차트 초기화용)
            public bool FirstDataReceived => FirstSymbol != null;       // 데이터가 한 번이라도 들어왔는지 확인

            // 🔥 [핵심 추가] 전체 종목 리스트를 차트 화면 등으로 넘겨주기 위한 메서드입니다! (이전 에러 해결의 키포인트)
            public List<JubbyStockInfo> GetStockList()
            {
                if (_stocks == null) return new List<JubbyStockInfo>(); // 딕셔너리가 비어있으면 에러 없이 빈 리스트 반환
                return _stocks.Values.ToList(); // 딕셔너리의 '값'들만 쏙 빼서 리스트 형태로 변환하여 반환
            }

            // 특정 종목코드(예: "005930")를 검색해서 그 종목의 객체를 찾아줍니다.
            public JubbyStockInfo GetStock(string symbol)
            {
                if (_stocks == null) return null;
                if (_stocks.TryGetValue(symbol, out JubbyStockInfo info)) return info; // 찾으면 반환
                return null; // 못 찾으면 null 반환
            }

            // 파이썬으로부터 JSON 메시지가 도착하면 이 함수가 호출되어 데이터를 해석합니다.
            public void HandleMessage(JsonMessage msg)
            {
                try
                {
                    // 'heartbeat'는 연결 유지용 더미 데이터이므로 쿨하게 무시합니다.
                    if (msg.MsgType.Equals("heartbeat", StringComparison.OrdinalIgnoreCase)) return;

                    // 🚨 [강력 진단용 알림벨] 파이썬 통신 연결 직후 딱 1번만 환영 팝업을 띄웁니다.
                    if (FirstSymbol == null && msg.MsgType.ToLower() == "market")
                    {
                        System.Windows.Forms.MessageBox.Show(
                            $"[C# 통신 연결 대성공!]\n\n파이썬에서 보낸 데이터가 C# 문을 열고 들어왔습니다!\n\n" +
                            $"메시지 종류: {msg.MsgType}\n" +
                            $"데이터 내용 일부: {msg.Payload?.ToString().Substring(0, Math.Min(100, msg.Payload.ToString().Length))}...",
                            "C# 수신 테스트"
                        );
                    }

                    // 메시지 타입(market, order 등)이 약속된 UpdateTarget이 아니면 거릅니다.
                    if (!Enum.TryParse<UpdateTarget>(msg.MsgType, true, out UpdateTarget target)) return;

                    // 데이터를 배열(JArray) 형태로 억지로라도 맞춰서 살려내는 구조대 코드입니다.
                    JArray payloadArray = msg.Payload as JArray;
                    if (payloadArray == null && msg.Payload != null)
                    {
                        try { payloadArray = JArray.FromObject(msg.Payload); } catch { }
                    }

                    // 정상적으로 배열 형태의 데이터를 확보했다면 종목별로 돌면서 업데이트합니다.
                    if (payloadArray != null)
                    {
                        foreach (JToken item in payloadArray)
                        {
                            string symbol = item["symbol"]?.ToString();
                            if (string.IsNullOrEmpty(symbol)) continue; // 종목 코드가 없으면 패스!

                            string name = item["symbol_name"]?.ToString() ?? symbol;

                            // 딕셔너리에 없는 새로운 종목이라면 새로 가입(new)시킵니다.
                            if (!_stocks.TryGetValue(symbol, out JubbyStockInfo info))
                            {
                                info = new JubbyStockInfo(symbol, name);
                                _stocks[symbol] = info;
                                if (FirstSymbol == null) FirstSymbol = symbol; // 첫 종목 등록!
                            }
                            else { info.Name = name; } // 이미 있으면 이름만 혹시 모르니 업데이트

                            // 알아낸 데이터를 해당 종목 객체에 예쁘게 덮어씌웁니다.
                            ApplyUpdate(info, target, item);


                            // 시장가(Market)이거나 전체(All) 업데이트인 경우, 차트 화면도 갱신하라고 명령합니다.
                            if (target == UpdateTarget.Market || target == UpdateTarget.All)
                            {
                                if (Auto.Ins.formGraphic != null)
                                {
                                    Auto.Ins.formGraphic.BeginInvoke(new Action(() => {
                                        Auto.Ins.formGraphic.UpdateMarketData(info);
                                    }));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 에러가 나면 콘솔이 아니라 팝업으로 띄워서 즉시 확인 가능하게 만듭니다.
                    System.Windows.Forms.MessageBox.Show($"[C# 수신 에러]\n{ex.Message}", "에러 확인");
                }
            }

            // JSON에서 넘어온 텍스트 숫자(예: "1,000", "5.5%")를 C# 계산용(Decimal)으로 깔끔하게 바꿔주는 헬퍼 함수
            private decimal ParseDecimal(JToken token, decimal defaultValue)
            {
                if (token == null) return defaultValue;
                // 콤마(,)와 퍼센트(%) 기호를 제거하고 공백을 지웁니다.
                string strVal = token.ToString().Replace(",", "").Replace("%", "").Trim();
                // 숫자로 변환 성공하면 변환값을, 실패하면 원래 가지고 있던 값을 그대로 씁니다.
                if (decimal.TryParse(strVal, out decimal result)) return result;
                return defaultValue;
            }

            // 타겟(분야)에 따라 알맞은 서랍장에 데이터를 착착 정리해줍니다.
            private void ApplyUpdate(JubbyStockInfo info, UpdateTarget target, JToken p)
            {
                switch (target)
                {
                    case UpdateTarget.Market: ApplyMarket(info.Market, p); break;       // 시장가 업데이트
                    case UpdateTarget.Account: ApplyAccount(info.MyAccount, p); break;  // 내 계좌 업데이트
                    case UpdateTarget.Order: ApplyOrder(info, p); break;                // 주문 기록 업데이트
                    case UpdateTarget.Strategy: ApplyStrategy(info.Strategy, p); break; // AI 지표 업데이트
                    case UpdateTarget.All: // 몽땅 다 업데이트
                        if (p["market"] != null) ApplyMarket(info.Market, p["market"]);
                        if (p["account"] != null) ApplyAccount(info.MyAccount, p["account"]);
                        if (p["order"] != null) ApplyOrder(info, p["order"]);
                        if (p["strategy"] != null) ApplyStrategy(info.Strategy, p["strategy"]);
                        break;
                }
            }

            // [시장가 데이터 매핑]
            private void ApplyMarket(TradeMarketData m, JToken p)
            {
                m.Last_Price = ParseDecimal(p["last_price"], m.Last_Price);
                m.Open_Price = ParseDecimal(p["open_price"], m.Open_Price);
                m.High_Price = ParseDecimal(p["high_price"], m.High_Price);
                m.Low_Price = ParseDecimal(p["low_price"], m.Low_Price);
                m.Return_1m = ParseDecimal(p["return_1m"], m.Return_1m);
                m.Trade_Amount = ParseDecimal(p["trade_amount"], m.Trade_Amount);
                m.Vol_Energy = ParseDecimal(p["vol_energy"], m.Vol_Energy);
                m.Disparity = ParseDecimal(p["disparity"], m.Disparity);
                m.Volume = ParseDecimal(p["volume"], m.Volume);
            }

            // [계좌 잔고 매핑]
            private void ApplyAccount(TradeAccountData acc, JToken p)
            {
                acc.Quantity = ParseDecimal(p["quantity"], acc.Quantity);
                acc.Avg_Price = ParseDecimal(p["avg_price"], acc.Avg_Price);
                acc.Current_Price = ParseDecimal(p["current_price"], acc.Current_Price);
                acc.Pnl_Amt = ParseDecimal(p["pnl_amt"], acc.Pnl_Amt);
                acc.Pnl_Rate = ParseDecimal(p["pnl_rate"], acc.Pnl_Rate);
                acc.Available_Cash = ParseDecimal(p["available_cash"], acc.Available_Cash);
            }

            // [주문 기록 매핑] (주문은 리스트에 하나씩 계속 쌓입니다)
            private void ApplyOrder(JubbyStockInfo info, JToken p)
            {
                TradeOrderData o = new TradeOrderData
                {
                    Order_Type = p["order_type"]?.ToString(),
                    Order_Price = ParseDecimal(p["order_price"], 0),
                    Order_Quantity = ParseDecimal(p["order_quantity"], 0),
                    Filled_Quqntity = ParseDecimal(p["filled_quantity"], 0),
                    Order_Time = p["order_time"]?.ToString(),
                    // 상태값은 대소문자가 다를 수 있어 여러 키값을 찔러봅니다.
                    Status = p["Status"]?.ToString() ?? p["order_status"]?.ToString() ?? "",
                    Order_Yield = p["order_yield"]?.ToString() ?? "0.00%"
                };
                info.AddOrder(o); // 만들어진 주문 객체를 종목의 주문 리스트에 추가!
            }

            // [AI 전략 지표 매핑]
            private void ApplyStrategy(TradeStrategyData s, JToken p)
            {
                s.Ma_5 = ParseDecimal(p["ma_5"], s.Ma_5);
                s.Ma_20 = ParseDecimal(p["ma_20"], s.Ma_20);
                s.RSI = ParseDecimal(p["rsi"], s.RSI);
                s.MACD = ParseDecimal(p["macd"], s.MACD);
                s.Signal = p["signal"]?.ToString() ?? s.Signal;
            }
        }
        #endregion

        #region ## Trade Data (세부 데이터 그릇 정의) ##
        // 각각의 데이터들이 담길 그릇(속성)들을 정의합니다.

        public class TradeMarketData
        {
            public decimal Last_Price { get; set; }   // 현재가
            public decimal Open_Price { get; set; }   // 시가
            public decimal High_Price { get; set; }   // 고가
            public decimal Low_Price { get; set; }    // 저가
            public decimal Return_1m { get; set; }    // 1분간 수익률 (등락률)
            public decimal Trade_Amount { get; set; } // 거래 대금
            public decimal Vol_Energy { get; set; }   // 체결 강도 (거래량 에너지)
            public decimal Disparity { get; set; }    // 이격도
            public decimal Volume { get; set; }       // 거래량
        }

        public class TradeAccountData
        {
            public decimal Quantity { get; set; }       // 보유 수량
            public decimal Avg_Price { get; set; }      // 매수 평균가
            public decimal Current_Price { get; set; }  // 현재 평가 금액
            public decimal Pnl_Amt { get; set; }        // 평가 손익 (원)
            public decimal Pnl_Rate { get; set; }       // 수익률 (%)
            public decimal Available_Cash { get; set; } // 매수 가능 예수금
        }

        public class TradeOrderData
        {
            public string Order_Type { get; set; }       // 주문 종류 (매수/매도/취소 등)
            public decimal Order_Price { get; set; }     // 주문 가격
            public decimal Order_Quantity { get; set; }  // 주문 수량
            public decimal Filled_Quqntity { get; set; } // 실제 체결된 수량
            public string Order_Time { get; set; }       // 주문 발생 시간
            public string Status { get; set; }           // 주문 상태 (접수, 체결 완료 등)
            public string Order_Yield { get; set; }      // 매도 시 기록되는 최종 수익률
        }

        public class TradeStrategyData
        {
            public decimal Ma_5 { get; set; }   // 5일(분) 이동평균선
            public decimal Ma_20 { get; set; }  // 20일(분) 이동평균선
            public decimal RSI { get; set; }    // 상대강도지수 (과매수/과매도 지표)
            public decimal MACD { get; set; }   // MACD 추세 지표
            public string Signal { get; set; }  // AI가 뱉어내는 최종 매매 시그널 (예: "강력매수")
        }
        #endregion

        // C# 화면(Form)들의 모드 번호와, 유저 등급에 대한 상수(고정값)를 정리해둔 곳입니다.
        public class ModeNumber { public const int Logout = 0; public const int Hide = 5; public const int Home = 10; public const int Simul = 20; public const int Auto = 30; public const int Error = 40; }
        public class UserLevel { public const int GUEST = 0; public const int ADMIN = 1; public const int MASTER = 2; }
    }
}