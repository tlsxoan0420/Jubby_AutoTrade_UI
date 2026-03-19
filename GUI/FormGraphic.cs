using Jubby_AutoTrade_UI.COMMON;
using Jubby_AutoTrade_UI.SEQUENCE;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// </summary>
        public void AddOrderMarker(string symbol, string type, double price)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return;

            // 이 종목 마커 바구니가 없으면 생성
            if (!OrderMarkers.ContainsKey(symbol))
                OrderMarkers[symbol] = new List<(double, double, string)>();

            // 매매가 체결된 '현재 시간'과 '가격', 그리고 '종류(BUY/SELL)'를 저장합니다.
            OrderMarkers[symbol].Add((DateTime.Now.ToOADate(), price, type));

            if (CurrentSelectedSymbol == symbol) isDataUpdated = true;
        }

        /// <summary>
        /// 🎨 실제로 현재 선택된 종목의 캔들 + 매매 마커(점)를 도화지에 그려냅니다.
        /// </summary>
        internal void LoadChart(string symbol)
        {
            var plt = FormsPlotMain.Plot;
            plt.Clear(); // 화면 깨끗이 비우기

            // 🕯️ 1. 캔들(양초) 그리기
            if (StockDataBuckets.ContainsKey(symbol))
            {
                var myBucket = StockDataBuckets[symbol];
                if (myBucket.Count > 0) plt.Add.Candlestick(myBucket);
            }

            // 🎯 2. 매매 타점(마커) 그리기
            if (OrderMarkers.ContainsKey(symbol))
            {
                var markers = OrderMarkers[symbol];

                // 🔴 BUY(매수) 타점 그리기: 빨간색 위쪽 화살표(▲)
                var buyX = markers.Where(m => m.type == "BUY").Select(m => m.time).ToArray();
                var buyY = markers.Where(m => m.type == "BUY").Select(m => m.price).ToArray();
                if (buyX.Length > 0)
                {
                    var spBuy = plt.Add.Scatter(buyX, buyY);
                    spBuy.LineStyle.Width = 0; // 선으로 잇지 않음
                    spBuy.MarkerStyle.Shape = MarkerShape.FilledTriangleUp;
                    spBuy.MarkerStyle.Size = 15;
                    spBuy.MarkerStyle.FillColor = Colors.Red;
                }

                // 🔵 SELL(매도) 타점 그리기: 파란색 아래쪽 화살표(▼)
                var sellX = markers.Where(m => m.type == "SELL").Select(m => m.time).ToArray();
                var sellY = markers.Where(m => m.type == "SELL").Select(m => m.price).ToArray();
                if (sellX.Length > 0)
                {
                    var spSell = plt.Add.Scatter(sellX, sellY);
                    spSell.LineStyle.Width = 0;
                    spSell.MarkerStyle.Shape = MarkerShape.FilledTriangleDown;
                    spSell.MarkerStyle.Size = 15;
                    spSell.MarkerStyle.FillColor = Colors.Blue;
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
    }
}