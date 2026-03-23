using Jubby_AutoTrade_UI.COMMON;
using Jubby_AutoTrade_UI.SEQUENCE;
using ScottPlot;
using ScottPlot.Finance;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Jubby_AutoTrade_UI.COMMON.Flag;

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormGraphic : Form
    {
        #region ## FormGraphic Define ##
        private FormsPlot FormsPlotMain;
        private List<Flag.JubbyStockInfo> StockList = new List<Flag.JubbyStockInfo>();
        private int CurrentIndex = 0;

        // 💡 화면 렌더링용 임시 리스트
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

        // 💡 [핵심 추가] "모든 종목"의 데이터를 독립적으로 보관하는 대형 금고
        private bool isBackupMode = false;
        private readonly Dictionary<string, List<OHLC>> LiveOHLCData = new Dictionary<string, List<OHLC>>();
        private readonly Dictionary<string, List<double>> LiveVolumeData = new Dictionary<string, List<double>>();
        private readonly Dictionary<string, List<OHLC>> BackupOHLCData = new Dictionary<string, List<OHLC>>();
        private readonly Dictionary<string, List<double>> BackupVolumeData = new Dictionary<string, List<double>>();
        #endregion ## FormGraphic Define ##

        public FormGraphic()
        {
            InitializeComponent();
            UI_Organize();
        }

        #region ## UI Organize ##
        private void UI_Organize()
        {
            this.KeyPreview = true;
            this.KeyDown += FormStockChart_KeyDown;

            FormsPlotMain = new ScottPlot.WinForms.FormsPlot
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(FormsPlotMain);

            FormsPlotMain.BringToFront();
            FormsPlotMain.MouseDown += FormsPlotMain_MouseDown;
            CreateCustomContextMenu();
        }
        #endregion ## UI Organize ##

        #region ## FormGraphic Load ##
        private void FormGraphic_Load(object sender, EventArgs e)
        {
            UI_Update();
        }
        #endregion ## FormGraphic Load ##

        #region ## UI Update ##
        private void UI_Update()
        {
            InitChart();
        }
        #endregion ## UI Update ##

        #region ## Custom Context Menu & Backtest ##
        private void CreateCustomContextMenu()
        {
            CustomChartMenu = new ContextMenuStrip();

            ToolStripMenuItem itemSave = new ToolStripMenuItem("데이터 및 이미지 백업 저장");
            itemSave.Click += (s, e) =>
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Title = "차트 백업 저장 (PNG 및 CSV 자동생성)";
                    sfd.Filter = "PNG 파일 (*.png)|*.png";
                    sfd.FileName = $"Jubby_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.png";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        SaveInteractiveChart(sfd.FileName);
                        MessageBox.Show("차트 이미지와 전체 종목 CSV 데이터가 함께 저장되었습니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            };

            ToolStripMenuItem itemAutoFit = new ToolStripMenuItem("자동 화면 맞춤 (Auto Fit)");
            itemAutoFit.Click += (s, e) =>
            {
                FormsPlotMain.Plot.Axes.AutoScale();
                FormsPlotMain.Refresh();
            };

            ToolStripMenuItem itemLoadBackup = new ToolStripMenuItem("백업 데이터 불러오기 (Load CSV)");
            itemLoadBackup.Click += LoadBackupData_Click;

            // =====================================================================
            // 🚀 [핵심 추가] C# 가상 백테스트 (과거 데이터 시뮬레이터)
            // =====================================================================
            ToolStripMenuItem itemBacktest = new ToolStripMenuItem("📊 [AI 시뮬레이션] 가상 백테스트 돌리기");
            itemBacktest.Click += (s, e) =>
            {
                if (!isBackupMode || BackupOHLCData.Count == 0)
                {
                    MessageBox.Show("먼저 '백업 데이터 불러오기'로 CSV 파일을 열어주세요!", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                RunVirtualBacktest();
            };

            CustomChartMenu.Items.Add(itemSave);
            CustomChartMenu.Items.Add(itemAutoFit);
            CustomChartMenu.Items.Add(new ToolStripSeparator());
            CustomChartMenu.Items.Add(itemLoadBackup);
            CustomChartMenu.Items.Add(new ToolStripSeparator());
            CustomChartMenu.Items.Add(itemBacktest); // 우클릭 메뉴에 백테스트 추가!
        }

        // 💡 [핵심 기능] 영수증을 출력해주는 가상 시뮬레이터 로직
        private void RunVirtualBacktest()
        {
            double starting_capital = 10000000; // 원금 1,000만 원 세팅
            double current_capital = starting_capital;
            int total_trades = 0;
            int win_trades = 0;
            int loss_trades = 0;

            // 로드된 전체 종목을 대상으로 가상 매매를 진행합니다.
            foreach (var symbol in BackupOHLCData.Keys)
            {
                var ohlc = BackupOHLCData[symbol];
                bool is_holding = false;
                double buy_price = 0;

                for (int i = 20; i < ohlc.Count; i++) // 20봉 이후부터 분석 시작
                {
                    var c = ohlc[i];
                    var prev = ohlc[i - 1];

                    // (가상 룰) 20일 이평선 돌파 시 매수, 2.5% 익절 / -1.5% 손절
                    double ma20 = ohlc.Skip(i - 20).Take(20).Average(x => x.Close);

                    if (!is_holding)
                    {
                        if (prev.Close < ma20 && c.Close > ma20) // 골든크로스 매수
                        {
                            is_holding = true;
                            buy_price = c.Close;
                        }
                    }
                    else
                    {
                        double profit_rate = ((c.Close - buy_price) / buy_price) * 100;
                        if (profit_rate >= 2.5 || profit_rate <= -1.5) // 목표가/손절가 도달
                        {
                            total_trades++;
                            if (profit_rate > 0) win_trades++;
                            else loss_trades++;

                            // 원금의 10%를 투자했다고 가정하고 수익금 합산
                            double invest_amt = current_capital * 0.1;
                            current_capital += invest_amt * (profit_rate / 100.0);

                            is_holding = false;
                        }
                    }
                }
            }

            // 결과 계산 및 영수증(리포트) 생성
            double total_profit = current_capital - starting_capital;
            double win_rate = total_trades > 0 ? ((double)win_trades / total_trades) * 100 : 0;

            string report = $"📊 [주삐 AI 백테스트 결과 영수증] 📊\n\n" +
                            $"💰 초기 자본금: {starting_capital:N0} 원\n" +
                            $"💵 최종 자본금: {current_capital:N0} 원\n" +
                            $"📈 순수익금: {total_profit:N0} 원\n\n" +
                            $"🔄 총 매매 횟수: {total_trades} 번\n" +
                            $"👑 승률: {win_rate:F1}%\n" +
                            $"🟢 익절: {win_trades} 번 / 🔴 손절: {loss_trades} 번";

            MessageBox.Show(report, "백테스트 결과", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FormsPlotMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && (Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                CustomChartMenu.Show(FormsPlotMain, e.Location);
            }
        }

        private void LoadBackupData_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "백업 차트 데이터 불러오기";
                ofd.Filter = "CSV 파일 (*.csv)|*.csv";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
                        if (lines.Length <= 1)
                        {
                            MessageBox.Show("데이터가 부족하거나 잘못된 파일입니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        BackupOHLCData.Clear();
                        BackupVolumeData.Clear();
                        var parsedOrders = new Dictionary<string, List<Flag.TradeOrderData>>();

                        for (int i = 1; i < lines.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(lines[i])) continue;
                            string[] cols = lines[i].Split(',');

                            if (cols.Length >= 6)
                            {
                                string symbol = cols[0].Trim();
                                if (DateTime.TryParse(cols[1].Trim(), out DateTime time))
                                {
                                    double.TryParse(cols[2], out double open);
                                    double.TryParse(cols[3], out double high);
                                    double.TryParse(cols[4], out double low);
                                    double.TryParse(cols[5], out double close);

                                    double volume = 0;
                                    string orderType = "";

                                    if (cols.Length > 6)
                                    {
                                        string val = cols[6].Trim();
                                        if (double.TryParse(val, out double v)) volume = v;
                                        else orderType = val;
                                    }

                                    if (open == 0 && close == 0) continue;

                                    if (!BackupOHLCData.ContainsKey(symbol))
                                    {
                                        BackupOHLCData[symbol] = new List<OHLC>();
                                        BackupVolumeData[symbol] = new List<double>();
                                        parsedOrders[symbol] = new List<Flag.TradeOrderData>();
                                    }

                                    // 중복 시간 데이터 건너뛰기
                                    if (BackupOHLCData[symbol].Any(x => x.DateTime == time)) continue;

                                    BackupOHLCData[symbol].Add(new OHLC(open, high, low, close, time, TimeSpan.FromMinutes(1)));
                                    BackupVolumeData[symbol].Add(volume);

                                    if (!string.IsNullOrEmpty(orderType))
                                    {
                                        parsedOrders[symbol].Add(new Flag.TradeOrderData
                                        {
                                            Order_Type = orderType,
                                            Order_Price = (decimal)close,
                                            Order_Time = time.ToString("yyyy-MM-dd HH:mm:00")
                                        });
                                    }
                                }
                            }
                        }

                        if (BackupOHLCData.Count == 0) return;

                        isBackupMode = true;
                        var newStockList = new List<Flag.JubbyStockInfo>();

                        foreach (var symbol in BackupOHLCData.Keys)
                        {
                            string realName = symbol;
                            var existingStock = Auto.Ins.GetStock(symbol);
                            if (existingStock != null && !string.IsNullOrWhiteSpace(existingStock.Name))
                                realName = existingStock.Name;

                            var stock = new Flag.JubbyStockInfo(symbol, realName);

                            CalculateIndicators(stock, BackupOHLCData[symbol], BackupVolumeData[symbol]);

                            if (parsedOrders.ContainsKey(symbol))
                                foreach (var order in parsedOrders[symbol])
                                    stock.AddOrder(order);

                            newStockList.Add(stock);
                            Auto.Ins.formDataChart?.SafeApplyStockUpdate(stock, Flag.UpdateTarget.All);
                        }

                        var bestSymbol = BackupOHLCData.OrderByDescending(x => x.Value.Count).First().Key;
                        this.StockList = newStockList;
                        this.CurrentIndex = newStockList.FindIndex(x => x.Symbol == bestSymbol);

                        LoadChart(this.StockList[this.CurrentIndex]);

                        MessageBox.Show($"총 {newStockList.Count}개의 종목을 추출하여\n실제 매매 표와 차트로 완벽하게 복원했습니다.", "복원 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void CalculateIndicators(Flag.JubbyStockInfo stock, List<OHLC> ohlcList, List<double> volList)
        {
            if (ohlcList.Count == 0) return;
            var last = ohlcList.Last();
            double lastClose = last.Close;
            double lastVol = volList.Last();

            stock.Market.Open_Price = (decimal)last.Open;
            stock.Market.High_Price = (decimal)last.High;
            stock.Market.Low_Price = (decimal)last.Low;
            stock.Market.Last_Price = (decimal)lastClose;
            stock.Market.Volume = (decimal)lastVol;

            if (ohlcList.Count > 1)
            {
                double prevClose = ohlcList[ohlcList.Count - 2].Close;
                stock.Market.Return_1m = prevClose > 0 ? (decimal)((lastClose - prevClose) / prevClose * 100.0) : 0m;
            }
            else if (last.Open > 0)
            {
                stock.Market.Return_1m = (decimal)((lastClose - last.Open) / last.Open * 100.0);
            }

            stock.Market.Trade_Amount = (decimal)(lastClose * lastVol / 1000000.0);

            if (volList.Count >= 5)
            {
                double volMa5 = volList.Skip(volList.Count - 5).Take(5).Average();
                stock.Market.Vol_Energy = volMa5 > 0 ? (decimal)(lastVol / volMa5) : 1m;
            }

            if (ohlcList.Count >= 5)
            {
                stock.Strategy.Ma_5 = (decimal)ohlcList.Skip(ohlcList.Count - 5).Take(5).Average(x => x.Close);
            }

            if (ohlcList.Count >= 20)
            {
                double ma20 = ohlcList.Skip(ohlcList.Count - 20).Take(20).Average(x => x.Close);
                stock.Strategy.Ma_20 = (decimal)ma20;
                stock.Market.Disparity = ma20 > 0 ? (decimal)(lastClose / ma20 * 100.0) : 100m;
            }

            if (ohlcList.Count > 14)
            {
                double sumGain = 0, sumLoss = 0;
                for (int i = ohlcList.Count - 14; i < ohlcList.Count; i++)
                {
                    double diff = ohlcList[i].Close - ohlcList[i - 1].Close;
                    if (diff > 0) sumGain += diff;
                    else sumLoss -= diff;
                }
                double avgLoss = sumLoss / 14.0;
                if (avgLoss == 0) stock.Strategy.RSI = 100m;
                else stock.Strategy.RSI = (decimal)(100.0 - (100.0 / (1.0 + (sumGain / 14.0) / avgLoss)));
            }

            if (ohlcList.Count > 26)
            {
                double ema12 = ohlcList.First().Close;
                double ema26 = ohlcList.First().Close;
                foreach (var c in ohlcList.Skip(1))
                {
                    ema12 = c.Close * (2.0 / 13.0) + ema12 * (1 - (2.0 / 13.0));
                    ema26 = c.Close * (2.0 / 27.0) + ema26 * (1 - (2.0 / 27.0));
                }
                stock.Strategy.MACD = (decimal)(ema12 - ema26);
            }

            stock.Strategy.Signal = "백업/복원";
        }

        private void ReDrawChartComplete()
        {
            if (OHLCList.Count == 0) return;

            if (CandlePlot != null) FormsPlotMain.Plot.Remove(CandlePlot);
            CandlePlot = FormsPlotMain.Plot.Add.Candlestick(OHLCList.ToArray());

            if (VolumePlot != null) FormsPlotMain.Plot.Remove(VolumePlot);
            VolumePlot = FormsPlotMain.Plot.Add.Bars(OrderHistoryList.ToArray());

            foreach (var bar in VolumePlot.Bars)
            {
                bar.FillColor = ScottPlot.Colors.Blue.WithAlpha(0.3);
                bar.LineWidth = 0;
            }
        }
        #endregion ## Custom Context Menu & Backtest ##

        #region ## Graphic Event ##

        internal void SetStockList(List<Flag.JubbyStockInfo> list)
        {
            StockList = list ?? new List<Flag.JubbyStockInfo>();
            CurrentIndex = 0;
            if (StockList.Count > 0) LoadChart(StockList[CurrentIndex]);
        }

        public void ShowStock(string symbol)
        {
            if (StockList == null || StockList.Count == 0) return;
            int idx = StockList.FindIndex(s => s.Symbol == symbol);
            if (idx == -1) return;
            CurrentIndex = idx;
            LoadChart(StockList[CurrentIndex]);
        }

        private void InitChart()
        {
            var plt = FormsPlotMain.Plot;
            FormsPlotMain.Menu?.Clear();

            plt.Axes.DateTimeTicksBottom();
            plt.Title("JUBBY STOCK CHART (데이터 대기 중...)");
            plt.YLabel("Price");
            ChartInitialized = true;
        }

        internal void LoadChart(Flag.JubbyStockInfo info)
        {
            if (!ChartInitialized) InitChart();

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

            ReDrawChartComplete();
            UpdateOrderMarkers(info.GetOrderListSafe());

            FormsPlotMain.Plot.Title($"JUBBY STOCK CHART - [{info.Name}]");
            FormsPlotMain.Plot.Axes.AutoScale();
            FormsPlotMain.Refresh();
        }

        private void UpdateOrderMarkers(List<Flag.TradeOrderData> orders)
        {
            var buyX = new List<double>(); var buyY = new List<double>();
            var sellX = new List<double>(); var sellY = new List<double>();

            if (orders == null) orders = new List<Flag.TradeOrderData>();

            foreach (var o in orders)
            {
                if (!DateTime.TryParse(o.Order_Time, out DateTime dt)) continue;

                double x = dt.ToOADate(); double y = (double)o.Order_Price;
                if (o.Order_Type.Equals("BUY", StringComparison.OrdinalIgnoreCase)) { buyX.Add(x); buyY.Add(y); }
                else if (o.Order_Type.Equals("SELL_PROFIT", StringComparison.OrdinalIgnoreCase) || o.Order_Type.Equals("SELL_LOSS", StringComparison.OrdinalIgnoreCase)) { sellX.Add(x); sellY.Add(y); }
            }

            if (BuyMarkers != null) FormsPlotMain.Plot.Remove(BuyMarkers);
            if (buyX.Count > 0)
            {
                BuyMarkers = FormsPlotMain.Plot.Add.Scatter(buyX.ToArray(), buyY.ToArray());
                BuyMarkers.Color = ScottPlot.Colors.Lime; BuyMarkers.MarkerShape = MarkerShape.FilledTriangleDown;
                BuyMarkers.MarkerSize = 8; BuyMarkers.LineWidth = 0;
            }
            else { BuyMarkers = null; }

            if (SellMarkers != null) FormsPlotMain.Plot.Remove(SellMarkers);
            if (sellX.Count > 0)
            {
                SellMarkers = FormsPlotMain.Plot.Add.Scatter(sellX.ToArray(), sellY.ToArray());
                SellMarkers.Color = ScottPlot.Colors.Red; SellMarkers.MarkerShape = MarkerShape.FilledTriangleUp;
                SellMarkers.MarkerSize = 8; SellMarkers.LineWidth = 0;
            }
            else { SellMarkers = null; }
        }
        #endregion ## Graphic Event ##

        #region ## UI Event ##
        private void FormStockChart_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Left) MovePrevStock();
            else if (e.Control && e.KeyCode == Keys.Right) MoveNextStock();
        }
        private void MovePrevStock()
        {
            if (StockList == null || StockList.Count == 0 || CurrentIndex <= 0) return;
            CurrentIndex--;
            LoadChart(StockList[CurrentIndex]);
        }
        private void MoveNextStock()
        {
            if (StockList == null || StockList.Count == 0 || CurrentIndex >= StockList.Count - 1) return;
            CurrentIndex++;
            LoadChart(StockList[CurrentIndex]);
        }
        #endregion ## UI Event ##

        #region ## Timer Event ##
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!Auto.Ins.DataManager.FirstDataReceived) return;
            if (!firstSetDone)
            {
                firstSetDone = true;
                string firstSymbol = Auto.Ins.DataManager.FirstSymbol;
                var firstInfo = Auto.Ins.GetStock(firstSymbol);
                if (firstInfo != null) SetStockList(new List<JubbyStockInfo> { firstInfo });
                return;
            }
            if (StockList == null || StockList.Count == 0 || CurrentIndex < 0 || CurrentIndex >= StockList.Count) return;

            string symbol = StockList[CurrentIndex].Symbol;
            var info = Auto.Ins.GetStock(symbol);
            if (info == null) return;

            if (!isBackupMode) LoadChart(info);
        }
        #endregion ## Timer Event ##

        #region ## 외부 호출 및 데이터 수신용 ##

        internal void UpdateMarketData(Flag.JubbyStockInfo info)
        {
            isBackupMode = false;
            if (info == null || info.Market == null) return;

            string sym = info.Symbol;
            if (!LiveOHLCData.ContainsKey(sym))
            {
                LiveOHLCData[sym] = new List<OHLC>();
                LiveVolumeData[sym] = new List<double>();
            }

            var m = info.Market;
            if (m.Open_Price > 0 && m.Last_Price > 0)
            {
                DateTime now = DateTime.Now;
                DateTime minuteTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

                var ohlcList = LiveOHLCData[sym];
                var volList = LiveVolumeData[sym];

                if (ohlcList.Count > 0 && ohlcList.Last().DateTime == minuteTime)
                {
                    var last = ohlcList.Last();
                    last.High = Math.Max(last.High, (double)m.High_Price);
                    last.Low = Math.Min(last.Low, (double)m.Low_Price);
                    last.Close = (double)m.Last_Price;
                    volList[volList.Count - 1] = (double)m.Volume;
                }
                else
                {
                    ohlcList.Add(new OHLC((double)m.Open_Price, (double)m.High_Price, (double)m.Low_Price, (double)m.Last_Price, minuteTime, TimeSpan.FromMinutes(1)));
                    volList.Add((double)m.Volume);

                    if (ohlcList.Count > MaxBars) ohlcList.RemoveAt(0);
                    if (volList.Count > MaxBars) volList.RemoveAt(0);
                }
            }

            if (StockList != null && StockList.Count > 0 && CurrentIndex < StockList.Count)
            {
                if (StockList[CurrentIndex].Symbol == sym)
                {
                    LoadChart(info);
                }
            }
        }

        internal void AddOrderMarker(string symbol, string orderType, decimal price, string time)
        {
            if (string.IsNullOrEmpty(symbol)) return;
            if (StockList != null && StockList.Count > 0 && CurrentIndex < StockList.Count)
            {
                if (StockList[CurrentIndex].Symbol == symbol)
                {
                    var info = StockList[CurrentIndex];
                    Flag.TradeOrderData newOrder = new Flag.TradeOrderData
                    {
                        Order_Type = orderType,
                        Order_Price = price,
                        Order_Time = time
                    };

                    info.AddOrder(newOrder);
                    UpdateOrderMarkers(info.GetOrderListSafe());
                    FormsPlotMain.Refresh();
                }
            }
        }

        internal void SaveInteractiveChart(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;
            try
            {
                FormsPlotMain.Plot.SavePng(filePath, 1200, 800);

                string csvPath = filePath.Replace(".png", ".csv");
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("종목코드,시간,시가,고가,저가,종가,거래발생(타점)");

                foreach (var symbol in LiveOHLCData.Keys)
                {
                    var ohlcList = LiveOHLCData[symbol];
                    var volList = LiveVolumeData[symbol];
                    var stockInfo = Auto.Ins.GetStock(symbol);
                    var orders = stockInfo?.GetOrderListSafe() ?? new List<Flag.TradeOrderData>();

                    for (int i = 0; i < ohlcList.Count; i++)
                    {
                        var c = ohlcList[i];
                        double v = volList.Count > i ? volList[i] : 0;

                        string orderStr = "";
                        var timeMatch = orders.FirstOrDefault(o => {
                            if (DateTime.TryParse(o.Order_Time, out DateTime odt))
                                return odt.ToString("yyyy-MM-dd HH:mm") == c.DateTime.ToString("yyyy-MM-dd HH:mm");
                            return false;
                        });

                        if (timeMatch != null) orderStr = timeMatch.Order_Type;

                        sb.AppendLine($"{symbol},{c.DateTime:yyyy-MM-dd HH:mm:00},{c.Open},{c.High},{c.Low},{c.Close},{orderStr}");
                    }
                }
                File.WriteAllText(csvPath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[차트 저장 오류] {ex.Message}");
            }
        }
        #endregion ## 외부 호출용 ##
    }
}