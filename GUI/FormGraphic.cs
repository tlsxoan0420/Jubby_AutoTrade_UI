using Jubby_AutoTrade_UI.COMMON;
using Jubby_AutoTrade_UI.SEQUENCE;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static Jubby_AutoTrade_UI.COMMON.Flag;

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormGraphic : Form
    {
        #region ## [1] 데이터 보관함 및 변수 설정 (Data Storage) ##

        private FormsPlot FormsPlotMain;

        /// <summary>
        /// 💡 [핵심 해결책] 종목별 캔들(양초) 데이터 '개별 바구니'입니다.
        /// "005930(삼성전자)"라는 이름표가 붙은 전용 리스트를 따로 관리하여 종목 캔들이 섞이는 것을 차단합니다.
        /// </summary>
        private Dictionary<string, List<OHLC>> StockDataBuckets = new Dictionary<string, List<OHLC>>();

        /// <summary>
        /// 🎯 [핵심 기능] 종목별 매수(BUY)/매도(SELL) 타점을 기억하는 마커 바구니입니다!
        /// 데이터 구성: (시간[OADate], 가격, "BUY" or "SELL")
        /// </summary>
        private Dictionary<string, List<(double time, double price, string type)>> OrderMarkers = new Dictionary<string, List<(double time, double price, string type)>>();

        /// <summary>
        /// 📈 [신규 기능] 종목별 '평균 매입가(평단가)'를 기억하는 바구니입니다!
        /// </summary>
        private Dictionary<string, double> AveragePrices = new Dictionary<string, double>();

        /// <summary>
        /// 💡 현재 사용자가 화면으로 보고 있는 '종목 코드'입니다. (표에서 클릭한 종목)
        /// </summary>
        public string CurrentSelectedSymbol = "";

        /// <summary>
        /// 차트 초기 설정이 완료되었는지 확인하는 변수입니다.
        /// </summary>
        private bool ChartInitialized = false;

        /// <summary>
        /// 종목당 최대 저장할 캔들 데이터 개수입니다. (최신 1000개 유지)
        /// </summary>
        private const int MaxBars = 1000;

        /// <summary>
        /// "새 데이터가 왔으니 현재 보고 있는 화면을 갱신해라!"라는 신호등입니다.
        /// </summary>
        private bool isDataUpdated = false;

        #endregion

        public FormGraphic()
        {
            InitializeComponent();
            UI_Organize(); // 차트 컨트롤 배치 및 기본 공사
        }

        private void UI_Organize()
        {
            FormsPlotMain = new ScottPlot.WinForms.FormsPlot();
            FormsPlotMain.Dock = DockStyle.Fill;
            palGrapic1.Controls.Add(FormsPlotMain); // 패널에 차트 박기
        }

        private void FormGraphic_Load(object sender, EventArgs e)
        {
            InitChart(); // 초기 디자인 설정
        }

        #region ## [2] 차트 초기 디자인 설정 (시간 표시) ##

        private void InitChart()
        {
            var plt = FormsPlotMain.Plot;

            // 💡 [영어 표기] 한글 깨짐 방지를 위해 영문 레이블을 사용합니다.
            plt.Title("Jubby AI Real-time Market Chart");
            plt.YLabel("Price (KRW)");

            // ✨ [X축 해결] X축을 단순 숫자가 아닌 '실제 시간(DateTime)' 포맷으로 그립니다!
            plt.Axes.DateTimeTicksBottom();

            ChartInitialized = true;
        }

        #endregion

        #region ## [3] 실시간 데이터 캔들 & 마커 그리기 (종목 독립 관리) ##

        /// <summary>
        /// 📡 파이썬에서 시세 데이터(market)가 오면 호출됩니다.
        /// 종목별 바구니에 캔들을 나누어 담습니다.
        /// </summary>
        internal void UpdateMarketData(Flag.JubbyStockInfo info)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdateMarketData(info)));
                return;
            }

            if (!ChartInitialized) InitChart();

            // 1. [바구니 생성] 이 종목의 바구니가 없다면 새로 만들어줍니다.
            if (!StockDataBuckets.ContainsKey(info.Symbol))
            {
                StockDataBuckets[info.Symbol] = new List<OHLC>();
            }

            // 💡 [신규] 파이썬에서 넘어온 '평균 매입가'를 전용 바구니에 업데이트합니다.
            if (info.MyAccount != null)
            {
                AveragePrices[info.Symbol] = (double)info.MyAccount.Avg_Price;
            }

            // =========================================================================
            // 💡 [문제 해결] 파이썬에서 데이터 누락/0원 수신 시 캔들 찌그러짐 방지 로직
            // =========================================================================
            double open = (double)info.Market.Open_Price;
            double high = (double)info.Market.High_Price;
            double low = (double)info.Market.Low_Price;
            double close = (double)info.Market.Last_Price; // 현재가

            // 파이썬에서 시가/고가/저가를 0으로 보냈다면, 전부 '현재가'로 통일하여 'ㅡ' 모양 캔들 생성
            if (open == 0) open = close;
            if (high == 0) high = close;
            if (low == 0) low = close;

            // 차트 라이브러리의 엄격한 규칙(고가가 제일 커야 함 등) 강제 만족
            high = Math.Max(high, Math.Max(open, close));
            low = Math.Min(low, Math.Min(open, close));

            // 2. [데이터 포장] 현재 '진짜 컴퓨터 시간'을 사용하여 1분짜리 캔들을 만듭니다.
            var ohlc = new OHLC(
                open,
                high,
                low,
                close,
                DateTime.Now, // 💡 현재 진짜 시간 사용
                TimeSpan.FromMinutes(1) // 1분봉 두께
            );

            // 3. [개별 바구니에 저장] 섞이지 않게 이 종목 전용 리스트에만 넣습니다.
            var myBucket = StockDataBuckets[info.Symbol];
            myBucket.Add(ohlc);
            if (myBucket.Count > MaxBars) myBucket.RemoveAt(0);

            // 4. [신호등 켜기] 지금 화면에 띄워둔 종목의 데이터가 왔다면 화면 갱신 예약!
            if (CurrentSelectedSymbol == info.Symbol)
            {
                isDataUpdated = true;
            }
        }

        /// <summary>
        /// 🎯 [핵심 기능] 파이썬에서 매수/매도를 때리면 그 흔적(타점)을 기록합니다!
        /// (과거 데이터 복원 시, 파이썬이 보내준 과거 시간을 반영하여 정확한 위치에 찍습니다.)
        /// </summary>
        public void AddOrderMarker(string symbol, string type, double price, string timeStr = "")
        {
            if (string.IsNullOrWhiteSpace(symbol)) return;

            if (!OrderMarkers.ContainsKey(symbol))
                OrderMarkers[symbol] = new List<(double, double, string)>();

            // 💡 [과거 시간 복원] 파이썬에서 넘어온 "2026-03-20 13:07:59" 같은 문자열을 진짜 시간으로 변환
            DateTime orderTime = DateTime.Now;
            if (!string.IsNullOrEmpty(timeStr))
            {
                DateTime.TryParse(timeStr, out orderTime);
            }

            double oaDate = orderTime.ToOADate();

            // 💡 [중복 점 찍기 방지] 이미 그 시간에 똑같은 화살표가 그려져 있다면 무시합니다.
            bool isExist = OrderMarkers[symbol].Any(m => m.time == oaDate && m.type == type);
            if (!isExist)
            {
                OrderMarkers[symbol].Add((oaDate, price, type));
                if (CurrentSelectedSymbol == symbol) isDataUpdated = true;
            }
        }

        /// <summary>
        /// 🎨 실제로 현재 선택된 종목의 캔들 + 매매 마커(점) + 평단가 선을 도화지에 그려냅니다.
        /// </summary>
        internal void LoadChart(string symbol)
        {
            var plt = FormsPlotMain.Plot;
            plt.Clear(); // 화면 깨끗이 비우기

            // 🕯️ 1. 캔들(양초) 그리기
            if (StockDataBuckets.ContainsKey(symbol))
            {
                var myBucket = StockDataBuckets[symbol];
                if (myBucket.Count > 0)
                {
                    var candlePlot = plt.Add.Candlestick(myBucket);
                }
            }

            // 🎯 2. 매매 타점(마커) 그리기 (3색상 분리 완벽 적용)
            if (OrderMarkers.ContainsKey(symbol))
            {
                var markers = OrderMarkers[symbol];

                // 🔵 BUY(매수) 타점 그리기: 파란색 위쪽 화살표(▲)
                var buyMarkers = markers.Where(m => m.type == "BUY").ToList();
                if (buyMarkers.Count > 0)
                {
                    double[] buyX = buyMarkers.Select(m => m.time).ToArray();
                    double[] buyY = buyMarkers.Select(m => m.price).ToArray();
                    var spBuy = plt.Add.Scatter(buyX, buyY);
                    spBuy.LineStyle.Width = 0; // 선으로 잇지 않음
                    spBuy.MarkerStyle.Shape = MarkerShape.FilledTriangleUp;
                    spBuy.MarkerStyle.Size = 15;
                    spBuy.MarkerStyle.FillColor = Colors.Blue;
                }

                // 🟢 SELL_PROFIT(익절) 타점 그리기: 라임색 아래쪽 화살표(▼)
                var profitMarkers = markers.Where(m => m.type == "SELL_PROFIT").ToList();
                if (profitMarkers.Count > 0)
                {
                    double[] sellX = profitMarkers.Select(m => m.time).ToArray();
                    double[] sellY = profitMarkers.Select(m => m.price).ToArray();
                    var spSellProfit = plt.Add.Scatter(sellX, sellY);
                    spSellProfit.LineStyle.Width = 0;
                    spSellProfit.MarkerStyle.Shape = MarkerShape.FilledTriangleDown;
                    spSellProfit.MarkerStyle.Size = 15;
                    spSellProfit.MarkerStyle.FillColor = Colors.Lime;
                }

                // 🔴 SELL_LOSS(손절 및 기타매도) 타점 그리기: 빨간색 아래쪽 화살표(▼)
                var lossMarkers = markers.Where(m => m.type == "SELL_LOSS" || m.type == "SELL").ToList();
                if (lossMarkers.Count > 0)
                {
                    double[] sellX = lossMarkers.Select(m => m.time).ToArray();
                    double[] sellY = lossMarkers.Select(m => m.price).ToArray();
                    var spSellLoss = plt.Add.Scatter(sellX, sellY);
                    spSellLoss.LineStyle.Width = 0;
                    spSellLoss.MarkerStyle.Shape = MarkerShape.FilledTriangleDown;
                    spSellLoss.MarkerStyle.Size = 15;
                    spSellLoss.MarkerStyle.FillColor = Colors.Red;
                }
            }

            // 📈 3. 평균 매입가(평단가) 선 그리기 [신규 기능!]
            if (AveragePrices.ContainsKey(symbol))
            {
                double avgPrice = AveragePrices[symbol];

                // 평단가가 0보다 클 때(즉, 진짜 보유하고 있을 때)만 그립니다.
                if (avgPrice > 0)
                {
                    var hline = plt.Add.HorizontalLine(avgPrice);
                    hline.LineStyle.Color = Colors.Magenta;            // 강렬한 마젠타(분홍) 색상
                    hline.LineStyle.Width = 2;                         // 두께 2
                    hline.LineStyle.Pattern = LinePattern.Dashed;      // 점선으로 표시하여 캔들과 구분
                }
            }

            // 화면 크기에 맞게 자동 줌 인!
            plt.Axes.AutoScale();
            FormsPlotMain.Refresh(); // 최종 렌더링
        }

        #endregion

        #region ## [4] 종목 전환 및 외부 통신 (Interface) ##

        /// <summary>
        /// 표(DataGrid)에서 종목을 클릭했을 때 호출되어 차트를 싹 바꿔줍니다.
        /// </summary>
        public void ShowStock(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return;

            CurrentSelectedSymbol = symbol; // "이제 이 종목만 보여줘" 라고 설정
            LoadChart(symbol); // 즉시 화면 갱신
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // 타이머가 돌다가 업데이트 신호(isDataUpdated)가 켜져 있으면 화면을 갱신합니다.
            if (isDataUpdated && !string.IsNullOrEmpty(CurrentSelectedSymbol))
            {
                LoadChart(CurrentSelectedSymbol);
                isDataUpdated = false; // 신호등 끄기
            }
        }

        #endregion

        #region ## [5] 자동매매 종료 시 차트 데이터 백업 (CSV 추출) ##
        /// <summary>
        /// 💡 Auto.cs에서 Stop()이 불릴 때 실행됩니다.
        /// 지금까지 모인 캔들(OHLC)과 매매 타점(마커) 데이터를 나중에 분석할 수 있도록
        /// 엑셀(CSV) 파일로 예쁘게 뽑아서 저장합니다.
        /// </summary>
        public void SaveInteractiveChart()
        {
            try
            {
                // 1. 저장할 폴더 만들기 (프로그램 폴더 안의 ChartDataBackup 폴더)
                string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string directoryPath = Path.Combine(Application.StartupPath, "ChartDataBackup");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string csvFilePath = Path.Combine(directoryPath, $"Jubby_TradeRecord_{timeStamp}.csv");

                // 2. CSV 파일 쓰기 시작 (BOM을 추가하여 한글 깨짐 방지)
                using (StreamWriter writer = new StreamWriter(csvFilePath, false, new UTF8Encoding(true)))
                {
                    // 헤더(열 이름) 작성
                    writer.WriteLine("종목코드,시간,시가,고가,저가,종가,거래발생(타점)");

                    // 3. 기록된 모든 종목의 캔들을 뒤지면서 기록
                    foreach (var bucket in StockDataBuckets)
                    {
                        string symbol = bucket.Key;
                        List<OHLC> candles = bucket.Value;

                        // 이 종목의 매매 타점(마커)이 있는지 확인
                        bool hasMarkers = OrderMarkers.ContainsKey(symbol);
                        List<(double time, double price, string type)> markers = hasMarkers ? OrderMarkers[symbol] : null;

                        foreach (var c in candles)
                        {
                            // 현재 캔들의 시간에 매수/매도 마커가 찍혀있는지 확인
                            string tradeAction = "";
                            if (hasMarkers)
                            {
                                // OADate를 DateTime으로 변환하여 시간 비교 (초 단위까지만 비교)
                                var markerInThisCandle = markers.FirstOrDefault(m =>
                                    DateTime.FromOADate(m.time).ToString("HH:mm:ss") == c.DateTime.ToString("HH:mm:ss"));

                                if (markerInThisCandle.type != null)
                                {
                                    tradeAction = markerInThisCandle.type; // "BUY" 또는 "SELL_PROFIT", "SELL_LOSS" 기록
                                }
                            }

                            // CSV 한 줄 만들기
                            string line = $"{symbol},{c.DateTime:yyyy-MM-dd HH:mm:ss},{c.Open},{c.High},{c.Low},{c.Close},{tradeAction}";
                            writer.WriteLine(line);
                        }
                    }
                }

                // UI 스레드에서 메시지박스 띄우기
                this.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show($"📈 딥러닝 복기용 차트 데이터가 성공적으로 백업되었습니다!\n\n저장 경로:\n{csvFilePath}",
                                    "차트 데이터 백업 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show($"차트 데이터를 저장하는 중 에러가 발생했습니다.\n{ex.Message}",
                                    "저장 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }
        #endregion
    }
}