using System;
using System.Data;
using System.IO;
using System.Timers; // 🔥 [추가] 0.5초마다 심장을 뛰게 만들 타이머
using System.Threading;
using System.Collections.Generic; // 🔥 [추가] 딕셔너리 사용
using System.Linq;                // 🔥 [추가] Linq 사용
using Jubby_AutoTrade_UI.COMMON;
using Jubby_AutoTrade_UI.GUI;
using Jubby_AutoTrade_UI.DATABASE; // 🔥 [추가] 파이썬 DB 리더기
using static Jubby_AutoTrade_UI.COMMON.Flag;

namespace Jubby_AutoTrade_UI.SEQUENCE
{
    // 외부 폼(Form)에서 Auto.Ins 로 접근할 수 있도록 public 선언
    public class Auto
    {
        #region ## Class Instance (싱글톤 패턴) ##
        // 어디서든 'Auto.Ins'를 통해 시스템 심장부에 접근할 수 있게 합니다.
        private static Auto _ins = new Auto();
        public static Auto Ins => _ins;
        #endregion ## Class Instance ##

        #region ## Auto Define (핵심 변수 정의) ##

        // 🟢 파이썬의 일기장(DB)을 읽어올 전담 직원
        public DB_Manager dbManager;

        // 🟢 0.5초마다 DB를 훔쳐보게 만들 자동 타이머
        private System.Timers.Timer dbPollingTimer;

        public bool Running = false;   // 전체 시스템 동작 상태 플래그

        // 기존 UI 창 관리 변수들 (화면 객체 미리 생성)
        public FormGraphic formGraphic = new FormGraphic();
        public FormDataChart formDataChart = new FormDataChart();
        #endregion ## Auto Define ##

        #region ## 📡 UI 업데이트용 이벤트 (신호탄) ##
        // 💡 0.5초마다 DB에서 퍼온 엑셀 표(DataTable)를 UI 화면에 던져줄 이벤트(신호탄)들입니다.
        public event Action<DataTable> OnMarketDataRefreshed;
        public event Action<DataTable> OnAccountDataRefreshed;
        public event Action<DataTable> OnStrategyDataRefreshed;
        public event Action<DataTable> OnOrderDataRefreshed;

        public event Action<DataTable> OnLogDataRefreshed;          // 파이썬 로그용
        public event Action<DataTable> OnSystemStatusRefreshed;     // 진행률(프로그레스바)용
        public event Action<DataTable> OnSharedSettingsRefreshed;   // 상단 설정 라벨용
        #endregion

        #region ## 📈 차트용 메모리 데이터 관리 (GetStock) ##
        // 🔥 DB에서 읽어온 데이터를 차트(FormGraphic)가 그리기 편하도록 객체로 담아두는 보관소입니다.
        private Dictionary<string, JubbyStockInfo> _memoryStocks = new Dictionary<string, JubbyStockInfo>();

        public List<JubbyStockInfo> GetStockList()
        {
            return _memoryStocks.Values.ToList();
        }

        public JubbyStockInfo GetStock(string symbol)
        {
            if (_memoryStocks.TryGetValue(symbol, out JubbyStockInfo info))
                return info;
            return null;
        }

        // DB에서 가져온 표(DataTable) 데이터를 차트용 객체로 변환해주는 헬퍼 함수

        private void UpdateMemoryStocks(DataTable marketDt, DataTable strategyDt, DataTable orderDt)
        {
            if (marketDt == null) return;

            var currentSymbols = new HashSet<string>(); // 현재 DB에 살아있는 종목들

            // 1. 시장가(Market) 갱신
            foreach (DataRow row in marketDt.Rows)
            {
                string symbol = row.Table.Columns.Contains("Symbol") ? row["Symbol"].ToString() : "";
                if (string.IsNullOrEmpty(symbol)) continue;

                currentSymbols.Add(symbol); // 생존 신고

                if (!_memoryStocks.ContainsKey(symbol))
                {
                    string name = row.Table.Columns.Contains("Name") ? row["Name"].ToString() : symbol;
                    _memoryStocks[symbol] = new JubbyStockInfo(symbol, name);
                }

                // 1. 시장가(Market) 갱신 부분에서 아래 줄들을 확인하세요.
                var m = _memoryStocks[symbol].Market;

                // 💡 [핵심] DB_Manager에서 'AS 별칭'으로 가져온 이름과 100% 똑같이 대소문자를 맞춰야 합니다!
                m.Last_Price = SafeGetDecimal(row, "Last_Price");
                m.Open_Price = SafeGetDecimal(row, "Open_Price");
                m.High_Price = SafeGetDecimal(row, "High_Price");
                m.Low_Price = SafeGetDecimal(row, "Low_Price");
                m.Volume = SafeGetDecimal(row, "Volume");
                m.Return_1m = SafeGetDecimal(row, "Return_1m"); // 👈 이 줄이 누락되었다면 추가하세요.
            }

            // 🔥 [버그 2 완벽 수정] 파이썬 DB에서 사라진 종목(매도 완료 등)은 C# 메모리에서도 즉시 삭제하여 차트와 표가 완벽히 동기화되게 합니다!
            var symbolsToRemove = _memoryStocks.Keys.Where(k => !currentSymbols.Contains(k)).ToList();
            foreach (var sym in symbolsToRemove)
            {
                _memoryStocks.Remove(sym);
            }

            // 2. 전략 시그널(Strategy) 갱신 (대략 70번째 줄)
            if (strategyDt != null)
            {
                foreach (DataRow row in strategyDt.Rows)
                {
                    string symbol = row.Table.Columns.Contains("Symbol") ? row["Symbol"].ToString() : "";
                    if (!string.IsNullOrEmpty(symbol) && _memoryStocks.ContainsKey(symbol))
                    {
                        var s = _memoryStocks[symbol].Strategy;
                        s.Signal = row.Table.Columns.Contains("Signal") ? row["Signal"].ToString() : s.Signal;
                        // 🔥 상태 메시지 매핑 추가
                        s.Status_Msg = row.Table.Columns.Contains("Status_Msg") ? row["Status_Msg"].ToString() : s.Status_Msg;
                    }
                }
            }

            // 3. 주문 기록(Order) 갱신 (대략 84번째 줄)
            if (orderDt != null)
            {
                foreach (var stock in _memoryStocks.Values) { stock.ClearOrders(); }
                foreach (DataRow row in orderDt.Rows)
                {
                    string symbol = row.Table.Columns.Contains("Symbol") ? row["Symbol"].ToString() : "";
                    if (!string.IsNullOrEmpty(symbol) && _memoryStocks.ContainsKey(symbol))
                    {
                        TradeOrderData o = new TradeOrderData
                        {
                            Order_No = row.Table.Columns.Contains("Order_No") ? row["Order_No"].ToString() : "", // 🔥 주문번호 매핑 추가
                            Order_Type = row.Table.Columns.Contains("Order_Type") ? row["Order_Type"].ToString() : "",
                            Order_Price = SafeGetDecimal(row, "Order_Price"),
                            Order_Time = row.Table.Columns.Contains("Order_Time") ? row["Order_Time"].ToString() : ""
                        };
                        _memoryStocks[symbol].AddOrder(o);
                    }
                }
            }
        }

        // 🔥 [수정된 방패 함수] DB 컬럼이 비어있거나 콤마(,)가 있어도 뻗지 않게 막아줍니다.
        private decimal SafeGetDecimal(DataRow row, string colName)
        {
            if (row.Table.Columns.Contains(colName) && row[colName] != DBNull.Value)
            {
                // 🔥 파이썬에서 넘어온 "8,370" 같은 콤마나 "%" 기호를 싹 지워야 정상적인 숫자로 바뀝니다!
                string cleanStr = row[colName].ToString().Replace(",", "").Replace("%", "").Trim();

                if (decimal.TryParse(cleanStr, out decimal result))
                    return result;
            }
            return 0;
        }
        #endregion

        /// <summary>
        /// [생성자] 시스템이 켜질 때 딱 한 번 실행되는 초기화 구간
        /// </summary>
        private Auto()
        {
            dbManager = new DB_Manager();

            // 500밀리초(0.5초) 간격으로 무한 반복되는 타이머 셋팅
            dbPollingTimer = new System.Timers.Timer(500);
            dbPollingTimer.Elapsed += DbPollingTimer_Elapsed;
            dbPollingTimer.AutoReset = true;
        }

        #region ## Auto Run (시스템 시작) ##
        public void Run()
        {
            if (Running) return;

            Running = true;

            Console.WriteLine("📡 [C# 시스템] SQLite DB 폴링 엔진 가동 시작!");
            dbPollingTimer.Start();

            formGraphic.timer1.Interval = 100;
            formGraphic.timer1.Enabled = true;
        }
        #endregion ## Auto Run ##

        #region ## Auto Stop (시스템 종료 및 백업) ##
        public void Stop()
        {
            if (!Running) return;

            Running = false;

            Console.WriteLine("🛑 [C# 시스템] SQLite DB 폴링 엔진 가동 중단!");
            dbPollingTimer.Stop();

            if (formGraphic != null && formGraphic.IsHandleCreated && !formGraphic.IsDisposed)
            {
                formGraphic.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        string backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Chart_Backups");
                        if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);

                        string fileName = $"Jubby_Chart_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        string fullPath = Path.Combine(backupFolder, fileName);

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

        #region ## ⚙️ 0.5초마다 실행되는 핵심 DB 읽기 엔진 ##
        private void DbPollingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                // 🔥 [핵심 안전장치] 데이터 읽는 도중 다음 0.5초가 도래해서 중복 실행되는 것을 방지!
                dbPollingTimer.Stop();

                DataTable marketDt = dbManager.GetMarketStatus();
                DataTable accountDt = dbManager.GetAccountStatus();
                DataTable strategyDt = dbManager.GetStrategyStatus();
                DataTable orderDt = dbManager.GetTradeHistory();

                DataTable logDt = dbManager.GetSharedLogs();
                DataTable statusDt = dbManager.GetSystemStatus();
                DataTable settingDt = dbManager.GetSharedSettings();

                //UpdateMemoryStocks(marketDt);
                // 📈 [핵심 연동] 차트 화면이 쓸 수 있도록 시장가, 전략, 주문 메모리에 최신화!
                UpdateMemoryStocks(marketDt, strategyDt, orderDt);

                // 2. 화면(Form) 쪽에 신호탄 발사! (UI 갱신)
                OnMarketDataRefreshed?.Invoke(marketDt);
                OnAccountDataRefreshed?.Invoke(accountDt);
                OnStrategyDataRefreshed?.Invoke(strategyDt);
                OnOrderDataRefreshed?.Invoke(orderDt);

                OnLogDataRefreshed?.Invoke(logDt);
                OnSystemStatusRefreshed?.Invoke(statusDt);
                OnSharedSettingsRefreshed?.Invoke(settingDt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB 폴링 중 가벼운 에러 발생 (무시됨): {ex.Message}");
            }
            finally
            {
                // 시스템이 실행(Running) 중이라면 타이머를 다시 가동합니다.
                if (Running)
                {
                    dbPollingTimer.Start();
                }
            }
        }
        #endregion
    }
}