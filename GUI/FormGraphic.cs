using Jubby_AutoTrade_UI.COMMON;
using Jubby_AutoTrade_UI.SEQUENCE;
using ScottPlot;
using ScottPlot.Finance;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static Jubby_AutoTrade_UI.COMMON.Flag;

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormGraphic : Form
    {
        private FormsPlot FormsPlotMain;
        private List<Flag.JubbyStockInfo> StockList = new List<Flag.JubbyStockInfo>();
        private int CurrentIndex = 0;
        private readonly List<OHLC> OHLCList = new List<OHLC>();
        private readonly List<double> OrderHistoryList = new List<double>();
        private CandlestickPlot CandlePlot;
        private BarPlot VolumePlot;
        private Scatter BuyMarkers;
        private Scatter SellMarkers;
        private bool ChartInitialized = false;
        private const int MaxBars = 500;
        private bool firstSetDone = false;
        private ContextMenuStrip CustomChartMenu;
        private bool isBackupMode = false;
        private readonly Dictionary<string, List<OHLC>> LiveOHLCData = new Dictionary<string, List<OHLC>>();
        private readonly Dictionary<string, List<double>> LiveVolumeData = new Dictionary<string, List<double>>();
        private readonly Dictionary<string, List<OHLC>> BackupOHLCData = new Dictionary<string, List<OHLC>>();
        private readonly Dictionary<string, List<double>> BackupVolumeData = new Dictionary<string, List<double>>();
        private bool isAutoAxis = true;
        private string lastLoadedSymbol = ""; // 🔥 [추가] 종목이 변경되었는지 감지하여 불필요한 차트 깜빡임을 막는 변수

        public FormGraphic() { InitializeComponent(); UI_Organize(); }

        private void UI_Organize()
        {
            this.KeyPreview = true; this.KeyDown += FormStockChart_KeyDown;
            FormsPlotMain = new ScottPlot.WinForms.FormsPlot { Dock = DockStyle.Fill };
            this.Controls.Add(FormsPlotMain); FormsPlotMain.BringToFront();
            FormsPlotMain.MouseDown += FormsPlotMain_MouseDown; CreateCustomContextMenu();
        }

        private void FormGraphic_Load(object sender, EventArgs e)
        {
            UI_Update();

            // 🔥 DB에서 새로운 데이터가 오면 차트 캔들 데이터(LiveOHLCData)를 축적하도록 신호탄 구독!
            Auto.Ins.OnMarketDataRefreshed += Auto_OnMarketDataRefreshed;
        }

        // 지휘관이 신호탄을 쏘면 실행되는 캔들 축적 함수
        private void Auto_OnMarketDataRefreshed(DataTable dt)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => Auto_OnMarketDataRefreshed(dt)));
                return;
            }

            try
            {
                var stocks = Auto.Ins.GetStockList();
                foreach (var stock in stocks)
                {
                    UpdateMarketData(stock);
                }
            }
            catch { }
        }

        private void UI_Update() { InitChart(); }

        private void CreateCustomContextMenu()
        {
            CustomChartMenu = new ContextMenuStrip();
            ToolStripMenuItem itemSave = new ToolStripMenuItem("데이터 및 이미지 백업 저장");
            itemSave.Click += (s, e) => {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Title = "차트 백업 저장 (PNG 및 CSV 자동생성)"; sfd.Filter = "PNG 파일 (*.png)|*.png"; sfd.FileName = $"Jubby_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    if (sfd.ShowDialog() == DialogResult.OK) { SaveInteractiveChart(sfd.FileName); MessageBox.Show("차트 이미지와 전체 종목 CSV 데이터가 함께 저장되었습니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                }
            };
            ToolStripMenuItem itemAutoFit = new ToolStripMenuItem("자동 화면 맞춤 (Auto Fit)");
            itemAutoFit.Click += (s, e) => { FormsPlotMain.Plot.Axes.AutoScale(); FormsPlotMain.Refresh(); };
            ToolStripMenuItem itemLoadBackup = new ToolStripMenuItem("백업 데이터 불러오기 (Load CSV)");
            itemLoadBackup.Click += LoadBackupData_Click;
            ToolStripMenuItem itemBacktest = new ToolStripMenuItem("📊 [AI 시뮬레이션] 가상 백테스트 돌리기");
            itemBacktest.Click += (s, e) => {
                if (!isBackupMode || BackupOHLCData.Count == 0) { MessageBox.Show("먼저 '백업 데이터 불러오기'로 CSV 파일을 열어주세요!", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                RunVirtualBacktest();
            };
            CustomChartMenu.Items.Add(new ToolStripSeparator()); // 구분선
            ToolStripMenuItem itemToggleAuto = new ToolStripMenuItem("실시간 자동 화면 맞춤 ON/OFF");
            itemToggleAuto.Click += (s, e) => {
                isAutoAxis = !isAutoAxis;
                itemToggleAuto.Checked = isAutoAxis;
                if (isAutoAxis) { FormsPlotMain.Plot.Axes.AutoScale(); FormsPlotMain.Refresh(); }
            };
            itemToggleAuto.Checked = true; // 기본값 ON
            CustomChartMenu.Items.Add(itemToggleAuto);
        }

        private void RunVirtualBacktest()
        {
            double starting_capital = 10000000; double current_capital = starting_capital; int total_trades = 0; int win_trades = 0; int loss_trades = 0;
            foreach (var symbol in BackupOHLCData.Keys)
            {
                var ohlc = BackupOHLCData[symbol]; bool is_holding = false; double buy_price = 0;
                for (int i = 20; i < ohlc.Count; i++)
                {
                    var c = ohlc[i]; var prev = ohlc[i - 1]; double ma20 = ohlc.Skip(i - 20).Take(20).Average(x => x.Close);
                    if (!is_holding)
                    {
                        if (prev.Close < ma20 && c.Close > ma20) { is_holding = true; buy_price = c.Close; }
                    }
                    else
                    {
                        double profit_rate = ((c.Close - buy_price) / buy_price) * 100;
                        if (profit_rate >= 2.5 || profit_rate <= -1.5)
                        {
                            total_trades++; if (profit_rate > 0) win_trades++; else loss_trades++;
                            double invest_amt = current_capital * 0.1; current_capital += invest_amt * (profit_rate / 100.0);
                            is_holding = false;
                        }
                    }
                }
            }
            double total_profit = current_capital - starting_capital; double win_rate = total_trades > 0 ? ((double)win_trades / total_trades) * 100 : 0;
            string report = $"📊 [주삐 AI 백테스트 결과 영수증] 📊\n\n💰 초기 자본금: {starting_capital:N0} 원\n💵 최종 자본금: {current_capital:N0} 원\n📈 순수익금: {total_profit:N0} 원\n\n🔄 총 매매 횟수: {total_trades} 번\n👑 승률: {win_rate:F1}%\n🟢 익절: {win_trades} 번 / 🔴 손절: {loss_trades} 번";
            MessageBox.Show(report, "백테스트 결과", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FormsPlotMain_MouseDown(object sender, MouseEventArgs e) { if (e.Button == MouseButtons.Right && (Control.ModifierKeys & Keys.Control) == Keys.Control) { CustomChartMenu.Show(FormsPlotMain, e.Location); } }

        private void LoadBackupData_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "백업 차트 데이터 불러오기"; ofd.Filter = "CSV 파일 (*.csv)|*.csv";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
                        if (lines.Length <= 1) { MessageBox.Show("데이터가 부족하거나 잘못된 파일입니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                        BackupOHLCData.Clear(); BackupVolumeData.Clear(); var parsedOrders = new Dictionary<string, List<Flag.TradeOrderData>>();
                        for (int i = 1; i < lines.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(lines[i])) continue;
                            string[] cols = lines[i].Split(',');
                            if (cols.Length >= 6)
                            {
                                string symbol = cols[0].Trim();
                                if (DateTime.TryParse(cols[1].Trim(), out DateTime time))
                                {
                                    double.TryParse(cols[2], out double open); double.TryParse(cols[3], out double high); double.TryParse(cols[4], out double low); double.TryParse(cols[5], out double close);
                                    double volume = 0; string orderType = "";
                                    if (cols.Length > 6) { string val = cols[6].Trim(); if (double.TryParse(val, out double v)) volume = v; else orderType = val; }
                                    if (open == 0 && close == 0) continue;
                                    if (!BackupOHLCData.ContainsKey(symbol)) { BackupOHLCData[symbol] = new List<OHLC>(); BackupVolumeData[symbol] = new List<double>(); parsedOrders[symbol] = new List<Flag.TradeOrderData>(); }
                                    if (BackupOHLCData[symbol].Any(x => x.DateTime == time)) continue;
                                    BackupOHLCData[symbol].Add(new OHLC(open, high, low, close, time, TimeSpan.FromMinutes(1))); BackupVolumeData[symbol].Add(volume);
                                    if (!string.IsNullOrEmpty(orderType)) { parsedOrders[symbol].Add(new Flag.TradeOrderData { Order_Type = orderType, Order_Price = (decimal)close, Order_Time = time.ToString("yyyy-MM-dd HH:mm:00") }); }
                                }
                            }
                        }
                        if (BackupOHLCData.Count == 0) return;
                        isBackupMode = true; var newStockList = new List<Flag.JubbyStockInfo>();
                        foreach (var symbol in BackupOHLCData.Keys)
                        {
                            string realName = symbol; var existingStock = Auto.Ins.GetStock(symbol);
                            if (existingStock != null && !string.IsNullOrWhiteSpace(existingStock.Name)) realName = existingStock.Name;
                            var stock = new Flag.JubbyStockInfo(symbol, realName); CalculateIndicators(stock, BackupOHLCData[symbol], BackupVolumeData[symbol]);
                            if (parsedOrders.ContainsKey(symbol)) foreach (var order in parsedOrders[symbol]) stock.AddOrder(order);
                            newStockList.Add(stock);
                        }
                        var bestSymbol = BackupOHLCData.OrderByDescending(x => x.Value.Count).First().Key; this.StockList = newStockList; this.CurrentIndex = newStockList.FindIndex(x => x.Symbol == bestSymbol);
                        LoadChart(this.StockList[this.CurrentIndex]); MessageBox.Show($"총 {newStockList.Count}개의 종목을 추출하여\n실제 매매 표와 차트로 완벽하게 복원했습니다.", "복원 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex) { MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        private void CalculateIndicators(Flag.JubbyStockInfo stock, List<OHLC> ohlcList, List<double> volList)
        {
            if (ohlcList.Count == 0) return;
            var last = ohlcList.Last(); double lastClose = last.Close; double lastVol = volList.Last();
            stock.Market.Open_Price = (decimal)last.Open; stock.Market.High_Price = (decimal)last.High; stock.Market.Low_Price = (decimal)last.Low; stock.Market.Last_Price = (decimal)lastClose; stock.Market.Volume = (decimal)lastVol;
            if (ohlcList.Count > 1) { double prevClose = ohlcList[ohlcList.Count - 2].Close; stock.Market.Return_1m = prevClose > 0 ? (decimal)((lastClose - prevClose) / prevClose * 100.0) : 0m; }
            else if (last.Open > 0) { stock.Market.Return_1m = (decimal)((lastClose - last.Open) / last.Open * 100.0); }
            stock.Market.Trade_Amount = (decimal)(lastClose * lastVol / 1000000.0);
            if (volList.Count >= 5) { double volMa5 = volList.Skip(volList.Count - 5).Take(5).Average(); stock.Market.Vol_Energy = volMa5 > 0 ? (decimal)(lastVol / volMa5) : 1m; }
            if (ohlcList.Count >= 5) { stock.Strategy.Ma_5 = (decimal)ohlcList.Skip(ohlcList.Count - 5).Take(5).Average(x => x.Close); }
            if (ohlcList.Count >= 20) { double ma20 = ohlcList.Skip(ohlcList.Count - 20).Take(20).Average(x => x.Close); stock.Strategy.Ma_20 = (decimal)ma20; stock.Market.Disparity = ma20 > 0 ? (decimal)(lastClose / ma20 * 100.0) : 100m; }
            if (ohlcList.Count > 14)
            {
                double sumGain = 0, sumLoss = 0;
                for (int i = ohlcList.Count - 14; i < ohlcList.Count; i++) { double diff = ohlcList[i].Close - ohlcList[i - 1].Close; if (diff > 0) sumGain += diff; else sumLoss -= diff; }
                double avgLoss = sumLoss / 14.0; if (avgLoss == 0) stock.Strategy.RSI = 100m; else stock.Strategy.RSI = (decimal)(100.0 - (100.0 / (1.0 + (sumGain / 14.0) / avgLoss)));
            }
            if (ohlcList.Count > 26) { double ema12 = ohlcList.First().Close; double ema26 = ohlcList.First().Close; foreach (var c in ohlcList.Skip(1)) { ema12 = c.Close * (2.0 / 13.0) + ema12 * (1 - (2.0 / 13.0)); ema26 = c.Close * (2.0 / 27.0) + ema26 * (1 - (2.0 / 27.0)); } stock.Strategy.MACD = (decimal)(ema12 - ema26); }
            stock.Strategy.Signal = "BACKUP";
        }

        private void ReDrawChartComplete()
        {
            // 🌟 [수정 4 - 차트 비우기 마법] 기존에 그려진 캔들이 있다면 무조건 화면에서 뽑아버립니다!
            if (CandlePlot != null)
            {
                FormsPlotMain.Plot.Remove(CandlePlot);
                CandlePlot = null;
            }

            // 🌟 [핵심] 만약 데이터가 0개라면? 새 캔들을 그리지 않고 여기서 즉시 함수를 끝냅니다.
            // 결과적으로 차트에는 아무것도 없는 자연스러운 빈 화면이 유지됩니다.
            if (OHLCList.Count == 0) return;

            // 데이터가 있을 때만 정상적으로 캔들 추가
            CandlePlot = FormsPlotMain.Plot.Add.Candlestick(OHLCList.ToArray());
        }

        internal void SetStockList(List<Flag.JubbyStockInfo> list) { StockList = list ?? new List<Flag.JubbyStockInfo>(); CurrentIndex = 0; if (StockList.Count > 0) LoadChart(StockList[CurrentIndex]); }
        public void ShowStock(string symbol) { if (StockList == null || StockList.Count == 0) return; int idx = StockList.FindIndex(s => s.Symbol == symbol); if (idx == -1) return; CurrentIndex = idx; LoadChart(StockList[CurrentIndex]); }

        // 🔥 [핵심 수정] 100% 영문화 적용으로 한글(ㅁㅁㅁ) 깨짐 완벽 방지!
        private void InitChart()
        {
            var plt = FormsPlotMain.Plot; FormsPlotMain.Menu?.Clear();
            plt.Axes.DateTimeTicksBottom();

            // 모든 텍스트를 반드시 영어로 고정
            plt.Title("JUBBY AI CHART - [Waiting for data...]");
            plt.YLabel("Price (KRW)");
            plt.XLabel("Time (HH:mm)");

            // 우측 상단 범례(Legend) 표시 켜기
            plt.Legend.IsVisible = true;

            ChartInitialized = true;
        }

        internal void LoadChart(Flag.JubbyStockInfo info)
        {
            try
            {
                if (!ChartInitialized) InitChart();

                // 🌟 [핵심 보정] 현재 보여줄 차트의 종목이 이전과 다르게 '바뀌었는지' 체크합니다.
                bool isSymbolChanged = (lastLoadedSymbol != info.Symbol);
                lastLoadedSymbol = info.Symbol;

                OHLCList.Clear();
                OrderHistoryList.Clear();

                if (isBackupMode && BackupOHLCData.ContainsKey(info.Symbol))
                {
                    OHLCList.AddRange(BackupOHLCData[info.Symbol]);
                    OrderHistoryList.AddRange(BackupVolumeData[info.Symbol]);
                }
                else if (!isBackupMode && LiveOHLCData.ContainsKey(info.Symbol))
                {
                    OHLCList.AddRange(LiveOHLCData[info.Symbol]);
                    OrderHistoryList.AddRange(LiveVolumeData[info.Symbol]);
                }

                // 🌟 [수정 2] 데이터가 0개면 차트가 비워지도록 지시합니다.
                ReDrawChartComplete();
                UpdateOrderMarkers(info.GetOrderListSafe());

                FormsPlotMain.Plot.Title($"JUBBY AI CHART - [{info.Symbol}]");

                // =============================================================
                // ⭐ [핵심 보정 3] "종목이 새로 클릭되었을 때"만 자동 화면 맞춤 실행!
                // 이렇게 하면 실시간 데이터가 1개씩 추가될 때 화면이 강제로 리셋되지 않아
                // 쾌적하게 마우스 휠로 확대/축소하면서 차트를 볼 수 있습니다.
                // =============================================================
                if (isAutoAxis && isSymbolChanged)
                {
                    FormsPlotMain.Plot.Axes.AutoScale();

                    if (OHLCList.Count > 0 && OHLCList.Count < 10)
                    {
                        double xLast = OHLCList.Last().DateTime.ToOADate();
                        FormsPlotMain.Plot.Axes.SetLimitsX(xLast - 0.01, xLast + 0.002);
                    }
                }

                FormsPlotMain.Refresh();
            }
            catch { }
        }

        private void UpdateOrderMarkers(List<Flag.TradeOrderData> orders)
        {
            var buyX = new List<double>(); var buyY = new List<double>(); var sellX = new List<double>(); var sellY = new List<double>();
            if (orders == null) orders = new List<Flag.TradeOrderData>();

            foreach (var o in orders)
            {
                if (string.IsNullOrEmpty(o.Order_Time) || !DateTime.TryParse(o.Order_Time, out DateTime dt)) continue;
                if (string.IsNullOrEmpty(o.Order_Type)) continue;

                double x = dt.ToOADate(); double y = (double)o.Order_Price;
                if (o.Order_Type.Contains("BUY") || o.Order_Type.Contains("매수")) { buyX.Add(x); buyY.Add(y); }
                else if (o.Order_Type.Contains("SELL") || o.Order_Type.Contains("매도") || o.Order_Type.Contains("절")) { sellX.Add(x); sellY.Add(y); }
            }

            if (BuyMarkers != null) FormsPlotMain.Plot.Remove(BuyMarkers);
            if (buyX.Count > 0)
            {
                BuyMarkers = FormsPlotMain.Plot.Add.Scatter(buyX.ToArray(), buyY.ToArray());
                BuyMarkers.Color = ScottPlot.Colors.Lime; // 매수 타점 라임색 삼각형
                BuyMarkers.MarkerShape = MarkerShape.FilledTriangleDown;
                BuyMarkers.MarkerSize = 10;
                BuyMarkers.LineWidth = 0;
                BuyMarkers.Label = "BUY"; // 범례 이름 영어로
            }
            else { BuyMarkers = null; }

            if (SellMarkers != null) FormsPlotMain.Plot.Remove(SellMarkers);
            if (sellX.Count > 0)
            {
                SellMarkers = FormsPlotMain.Plot.Add.Scatter(sellX.ToArray(), sellY.ToArray());
                SellMarkers.Color = ScottPlot.Colors.Red; // 매도 타점 빨간색 삼각형
                SellMarkers.MarkerShape = MarkerShape.FilledTriangleUp;
                SellMarkers.MarkerSize = 10;
                SellMarkers.LineWidth = 0;
                SellMarkers.Label = "SELL"; // 범례 이름 영어로
            }
            else { SellMarkers = null; }
        }

        private void FormStockChart_KeyDown(object sender, KeyEventArgs e) { if (e.Control && e.KeyCode == Keys.Left) MovePrevStock(); else if (e.Control && e.KeyCode == Keys.Right) MoveNextStock(); }
        private void MovePrevStock() { if (StockList == null || StockList.Count == 0 || CurrentIndex <= 0) return; CurrentIndex--; LoadChart(StockList[CurrentIndex]); }
        private void MoveNextStock() { if (StockList == null || StockList.Count == 0 || CurrentIndex >= StockList.Count - 1) return; CurrentIndex++; LoadChart(StockList[CurrentIndex]); }

        /// <summary>
        /// [타이머 이벤트] 실시간으로 차트를 갱신하거나 초기 데이터를 세팅합니다.
        /// </summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            var allStocks = Auto.Ins.GetStockList();
            if (allStocks == null || allStocks.Count == 0) return;

            // 1. 최초 1회 실행 시 (1개만 넣던 버그 수정!)
            if (!firstSetDone)
            {
                firstSetDone = true;
                // 🔥 [수정] 첫 번째 1개가 아니라, 가져온 '모든 종목 리스트'를 통째로 넘겨줍니다!
                SetStockList(allStocks);
                return;
            }

            // 2. 🔥 [추가] 중간에 새로운 종목이 수집/매수되어 리스트가 늘어났을 때의 동기화
            if (allStocks.Count != StockList.Count)
            {
                string curSymbol = StockList.Count > 0 ? StockList[CurrentIndex].Symbol : "";
                StockList = allStocks;
                int idx = StockList.FindIndex(x => x.Symbol == curSymbol);
                CurrentIndex = idx >= 0 ? idx : 0;
            }

            if (StockList == null || StockList.Count == 0 || CurrentIndex < 0 || CurrentIndex >= StockList.Count)
                return;

            // 3. 현재 차트에 표시 중인 종목
            string symbol = StockList[CurrentIndex].Symbol;

            var info = Auto.Ins.GetStock(symbol);
            if (info == null) return;

            if (!isBackupMode)
            {
                LoadChart(info);
            }
        }

        internal void UpdateMarketData(Flag.JubbyStockInfo info)
        {
            if (info == null || info.Market == null) return;
            isBackupMode = false;
            string sym = info.Symbol;

            if (!LiveOHLCData.ContainsKey(sym))
            {
                LiveOHLCData[sym] = new List<OHLC>();
                LiveVolumeData[sym] = new List<double>();
            }

            var m = info.Market;
            if (m.Last_Price > 0)
            {
                DateTime now = DateTime.Now;
                DateTime minuteTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                var ohlcList = LiveOHLCData[sym];
                var volList = LiveVolumeData[sym];

                double open = m.Open_Price <= 0 ? (double)m.Last_Price : (double)m.Open_Price;
                double high = m.High_Price <= 0 ? (double)m.Last_Price : (double)m.High_Price;
                double low = m.Low_Price <= 0 ? (double)m.Last_Price : (double)m.Low_Price;

                if (ohlcList.Count > 0 && ohlcList.Last().DateTime == minuteTime)
                {
                    var last = ohlcList.Last();
                    last.High = Math.Max(last.High, (double)m.High_Price);
                    last.Low = Math.Min(last.Low, (double)m.Low_Price);
                    if (last.Low <= 0) last.Low = (double)m.Last_Price;
                    last.Close = (double)m.Last_Price;
                    volList[volList.Count - 1] = (double)m.Volume;
                }
                else
                {
                    ohlcList.Add(new OHLC(open, high, low, (double)m.Last_Price, minuteTime, TimeSpan.FromMinutes(1)));
                    volList.Add((double)m.Volume);
                    if (ohlcList.Count > MaxBars) ohlcList.RemoveAt(0);
                    if (volList.Count > MaxBars) volList.RemoveAt(0);
                }
            }

            if (StockList.Count > 0 && CurrentIndex < StockList.Count)
            {
                if (StockList[CurrentIndex].Symbol == sym) LoadChart(info);
            }
        }

        internal void AddStockToList(Flag.JubbyStockInfo info)
        {
            if (!StockList.Any(x => x.Symbol == info.Symbol))
            {
                StockList.Add(info);
                if (StockList.Count == 1) ShowStock(info.Symbol);
            }
        }

        internal void AddOrderMarker(string symbol, string orderType, decimal price, string time)
        {
            if (string.IsNullOrEmpty(symbol)) return;
            if (StockList != null && StockList.Count > 0 && CurrentIndex < StockList.Count)
            {
                if (StockList[CurrentIndex].Symbol == symbol)
                {
                    var info = StockList[CurrentIndex]; Flag.TradeOrderData newOrder = new Flag.TradeOrderData { Order_Type = orderType, Order_Price = price, Order_Time = time };
                    info.AddOrder(newOrder); UpdateOrderMarkers(info.GetOrderListSafe()); FormsPlotMain.Refresh();
                }
            }
        }

        internal void SaveInteractiveChart(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;
            try
            {
                FormsPlotMain.Plot.SavePng(filePath, 1200, 800); string csvPath = filePath.Replace(".png", ".csv"); StringBuilder sb = new StringBuilder(); sb.AppendLine("종목코드,시간,시가,고가,저가,종가,거래발생(타점)");
                foreach (var symbol in LiveOHLCData.Keys)
                {
                    var ohlcList = LiveOHLCData[symbol]; var volList = LiveVolumeData[symbol]; var stockInfo = Auto.Ins.GetStock(symbol); var orders = stockInfo?.GetOrderListSafe() ?? new List<Flag.TradeOrderData>();
                    for (int i = 0; i < ohlcList.Count; i++)
                    {
                        var c = ohlcList[i]; double v = volList.Count > i ? volList[i] : 0; string orderStr = "";
                        var timeMatch = orders.FirstOrDefault(o => { if (DateTime.TryParse(o.Order_Time, out DateTime odt)) return odt.ToString("yyyy-MM-dd HH:mm") == c.DateTime.ToString("yyyy-MM-dd HH:mm"); return false; });
                        if (timeMatch != null) orderStr = timeMatch.Order_Type; sb.AppendLine($"{symbol},{c.DateTime:yyyy-MM-dd HH:mm:00},{c.Open},{c.High},{c.Low},{c.Close},{orderStr}");
                    }
                }
                File.WriteAllText(csvPath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex) { Console.WriteLine($"[차트 저장 오류] {ex.Message}"); }
        }
    }
}