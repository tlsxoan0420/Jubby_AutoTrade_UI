using Jubby_AutoTrade_UI.COM;
using Jubby_AutoTrade_UI.GUI;
using Jubby_AutoTrade_UI.SEQUENCE;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
        #endregion

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
        #endregion

        #region ## User Status##
        public class UserStatus
        {
            static public int Level;
            static public string Password;
            static public string Name;
            static public string LoginID;
        }
        #endregion

        #region ## Jubby Stock Info ##
        public enum UpdateTarget
        {
            Market, Account, Order, Strategy, All
        }

        public class JubbyStockInfo
        {
            public string Symbol { get; set; }
            public string Name { get; set; }
            public TradeMarketData Market { get; set; } = new TradeMarketData();
            public TradeAccountData MyAccount { get; set; } = new TradeAccountData();
            private readonly object OrderLock = new object();
            public List<TradeOrderData> OrderHistory { get; set; } = new List<TradeOrderData>();

            public void AddOrder(TradeOrderData order)
            {
                lock (OrderLock) { OrderHistory.Add(order); if (OrderHistory.Count > 100) OrderHistory.RemoveAt(0); }
            }
            public void RemoveOrder(string orderID)
            {
                lock (OrderLock) { OrderHistory.RemoveAll(x => x.Order_Type == orderID); }
            }
            public void ClearOrders()
            {
                lock (OrderLock) { OrderHistory.Clear(); }
            }
            public List<TradeOrderData> GetOrderListSafe()
            {
                lock (OrderLock) { return OrderHistory.ToList(); }
            }
            public TradeStrategyData Strategy { get; set; } = new TradeStrategyData();

            public JubbyStockInfo(string symbol, string name)
            {
                this.Symbol = symbol;
                this.Name = name;
            }
        }
        #endregion

        #region ## Jubby Data Manager ##
        public class JubbyDataManager
        {
            private readonly Dictionary<string, JubbyStockInfo> _stocks = new Dictionary<string, JubbyStockInfo>();
            public string FirstSymbol { get; private set; } = null;
            public bool FirstDataReceived => FirstSymbol != null;

            public JubbyStockInfo GetStock(string symbol)
            {
                if (_stocks == null) return null;
                if (_stocks.TryGetValue(symbol, out JubbyStockInfo info)) return info;
                return null;
            }

            public void HandleMessage(JsonMessage msg)
            {
                try
                {
                    if (msg.MsgType.Equals("heartbeat", StringComparison.OrdinalIgnoreCase)) return;

                    // 🚨 [강력 진단용 알림벨] 파이썬에서 데이터가 C#으로 넘어오면 무조건 팝업창을 띄웁니다! (최초 1회만)
                    if (FirstSymbol == null && msg.MsgType.ToLower() == "market")
                    {
                        System.Windows.Forms.MessageBox.Show(
                            $"[C# 통신 연결 대성공!]\n\n파이썬에서 보낸 데이터가 C# 문을 열고 들어왔습니다!\n\n" +
                            $"메시지 종류: {msg.MsgType}\n" +
                            $"데이터 내용 일부: {msg.Payload?.ToString().Substring(0, Math.Min(100, msg.Payload.ToString().Length))}...",
                            "C# 수신 테스트"
                        );
                    }

                    if (!Enum.TryParse<UpdateTarget>(msg.MsgType, true, out UpdateTarget target)) return;

                    // 데이터 형태가 JArray가 아니어도 억지로 변환해서 살려내는 코드
                    JArray payloadArray = msg.Payload as JArray;
                    if (payloadArray == null && msg.Payload != null)
                    {
                        try { payloadArray = JArray.FromObject(msg.Payload); } catch { }
                    }

                    if (payloadArray != null)
                    {
                        foreach (JToken item in payloadArray)
                        {
                            string symbol = item["symbol"]?.ToString();
                            if (string.IsNullOrEmpty(symbol)) continue;

                            string name = item["symbol_name"]?.ToString() ?? symbol;

                            if (!_stocks.TryGetValue(symbol, out JubbyStockInfo info))
                            {
                                info = new JubbyStockInfo(symbol, name);
                                _stocks[symbol] = info;
                                if (FirstSymbol == null) FirstSymbol = symbol;
                            }
                            else { info.Name = name; }

                            ApplyUpdate(info, target, item);

                            // 강제 화면 업데이트
                            if (Auto.Ins.formDataChart != null)
                            {
                                Auto.Ins.formDataChart.BeginInvoke(new Action(() => {
                                    Auto.Ins.formDataChart.SafeApplyStockUpdate(info, target);
                                }));
                            }

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
                    // 에러가 나면 콘솔이 아니라 팝업으로 띄워서 즉시 확인!
                    System.Windows.Forms.MessageBox.Show($"[C# 수신 에러]\n{ex.Message}", "에러 확인");
                }
            }

            private decimal ParseDecimal(JToken token, decimal defaultValue)
            {
                if (token == null) return defaultValue;
                string strVal = token.ToString().Replace(",", "").Replace("%", "").Trim();
                if (decimal.TryParse(strVal, out decimal result)) return result;
                return defaultValue;
            }

            private void ApplyUpdate(JubbyStockInfo info, UpdateTarget target, JToken p)
            {
                switch (target)
                {
                    case UpdateTarget.Market: ApplyMarket(info.Market, p); break;
                    case UpdateTarget.Account: ApplyAccount(info.MyAccount, p); break;
                    case UpdateTarget.Order: ApplyOrder(info, p); break;
                    case UpdateTarget.Strategy: ApplyStrategy(info.Strategy, p); break;
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

            private void ApplyAccount(TradeAccountData acc, JToken p)
            {
                acc.Quantity = ParseDecimal(p["quantity"], acc.Quantity);
                acc.Avg_Price = ParseDecimal(p["avg_price"], acc.Avg_Price);
                acc.Current_Price = ParseDecimal(p["current_price"], acc.Current_Price);
                acc.Pnl_Amt = ParseDecimal(p["pnl_amt"], acc.Pnl_Amt);
                acc.Pnl_Rate = ParseDecimal(p["pnl_rate"], acc.Pnl_Rate);
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
                    Status = p["Status"]?.ToString() ?? p["order_status"]?.ToString() ?? "",
                    Order_Yield = p["order_yield"]?.ToString() ?? "0.00%"
                };
                info.AddOrder(o);
            }

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

        #region ## Trade Data ##
        public class TradeMarketData
        {
            public decimal Last_Price { get; set; }
            public decimal Open_Price { get; set; }
            public decimal High_Price { get; set; }
            public decimal Low_Price { get; set; }
            public decimal Return_1m { get; set; }
            public decimal Trade_Amount { get; set; }
            public decimal Vol_Energy { get; set; }
            public decimal Disparity { get; set; }
            public decimal Volume { get; set; }
        }

        public class TradeAccountData
        {
            public decimal Quantity { get; set; }
            public decimal Avg_Price { get; set; }
            public decimal Current_Price { get; set; }
            public decimal Pnl_Amt { get; set; }
            public decimal Pnl_Rate { get; set; }
            public decimal Available_Cash { get; set; }
        }

        public class TradeOrderData
        {
            public string Order_Type { get; set; }
            public decimal Order_Price { get; set; }
            public decimal Order_Quantity { get; set; }
            public decimal Filled_Quqntity { get; set; }
            public string Order_Time { get; set; }
            public string Status { get; set; }
            public string Order_Yield { get; set; }
        }

        public class TradeStrategyData
        {
            public decimal Ma_5 { get; set; }
            public decimal Ma_20 { get; set; }
            public decimal RSI { get; set; }
            public decimal MACD { get; set; }
            public string Signal { get; set; }
        }
        #endregion

        public class ModeNumber { public const int Logout = 0; public const int Hide = 5; public const int Home = 10; public const int Simul = 20; public const int Auto = 30; public const int Error = 40; }
        public class UserLevel { public const int GUEST = 0; public const int ADMIN = 1; public const int MASTER = 2; }
    }
}