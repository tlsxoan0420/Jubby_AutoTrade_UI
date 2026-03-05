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
        /// 💡 [핵심 해결책] 종목별 데이터 '개별 바구니'입니다.
        /// "005930(삼성전자)"라는 이름표가 붙은 전용 리스트를 따로 관리하여 10개 종목 데이터가 섞이는 것을 원천 차단합니다.
        /// </summary>
        private Dictionary<string, List<OHLC>> StockDataBuckets = new Dictionary<string, List<OHLC>>();

        /// <summary>
        /// 💡 [핵심 해결책] 종목별 '개별 시간 카운터'입니다.
        /// 각 종목마다 0초, 1초, 2초... 흐르는 시간이 다르기 때문에 종목별로 숫자를 따로 기억합니다.
        /// </summary>
        private Dictionary<string, double> StockXCounters = new Dictionary<string, double>();

        /// <summary>
        /// 파이썬에서 보내온 전체 종목 정보 리스트입니다.
        /// </summary>
        private List<Flag.JubbyStockInfo> StockList = new List<Flag.JubbyStockInfo>();

        /// <summary>
        /// 현재 사용자가 화면으로 보고 있는 종목의 순번입니다.
        /// </summary>
        private int CurrentIndex = 0;

        /// <summary>
        /// 차트 초기 설정이 완료되었는지 확인하는 변수입니다.
        /// </summary>
        private bool ChartInitialized = false;

        /// <summary>
        /// 종목당 최대 저장할 데이터 개수입니다. (최신 500개)
        /// </summary>
        private const int MaxBars = 500;

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
            this.KeyPreview = true;
            this.KeyDown += FormStockChart_KeyDown; // 단축키 연결

            FormsPlotMain = new ScottPlot.WinForms.FormsPlot();
            FormsPlotMain.Dock = DockStyle.Fill;
            palGrapic1.Controls.Add(FormsPlotMain); // 패널에 차트 박기
        }

        private void FormGraphic_Load(object sender, EventArgs e)
        {
            InitChart(); // 초기 디자인 설정
        }

        #region ## [2] 차트 초기 디자인 설정 (영문 및 0sec 고정) ##

        private void InitChart()
        {
            var plt = FormsPlotMain.Plot;

            // 💡 [영어 표기] 한글 깨짐 방지를 위해 영문 레이블을 사용합니다.
            plt.XLabel("Elapsed Time (Seconds)");
            plt.YLabel("Price (KRW)");
            plt.Title("Jubby AI Real-time Market Chart");

            // ✨ [X축 해결] 차트 왼쪽 끝을 0으로 강제 고정하여 데이터가 없을 때 마이너스 숫자가 나오는 것을 막습니다.
            plt.Axes.SetLimitsX(0, 100);

            // 💡 [X축 라벨] 숫자 뒤에 "sec"를 자동으로 붙여주는 설정을 합니다.
            var tickGen = new ScottPlot.TickGenerators.NumericAutomatic();
            tickGen.LabelFormatter = (val) => val < 0 ? "" : $"{val}sec";
            plt.Axes.Bottom.TickGenerator = tickGen;

            ChartInitialized = true;
        }

        #endregion

        #region ## [3] 실시간 데이터 분리 수신 및 그리기 (10개 종목 독립 관리) ##

        /// <summary>
        /// 📡 파이썬에서 데이터가 오면 호출됩니다. 10개 종목을 각각의 바구니에 나누어 담습니다.
        /// </summary>
        internal void UpdateMarketData(Flag.JubbyStockInfo info)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdateMarketData(info)));
                return;
            }

            if (!ChartInitialized) InitChart();

            // 1. [바구니 생성 확인] 이 종목의 바구니가 처음이라면 새로 만들어줍니다.
            if (!StockDataBuckets.ContainsKey(info.Symbol))
            {
                StockDataBuckets[info.Symbol] = new List<OHLC>();
                StockXCounters[info.Symbol] = 0; // 이 종목만의 시계는 0부터 시작합니다.

                if (StockList.All(s => s.Symbol != info.Symbol)) StockList.Add(info);
            }

            // 2. [데이터 포장] 이 종목만의 카운터를 사용하여 캔들 데이터를 만듭니다.
            double currentX = StockXCounters[info.Symbol];
            StockXCounters[info.Symbol]++; // 다음 데이터를 위해 1을 더합니다.

            var ohlc = new OHLC(
                (double)info.Market.Open_Price,
                (double)info.Market.High_Price,
                (double)info.Market.Low_Price,
                (double)info.Market.Last_Price,
                DateTime.FromOADate(currentX), // 💡 개별 종목의 0, 1, 2... 좌표
                TimeSpan.FromDays(1)
            );

            // 3. [개별 바구니에 저장] 섞이지 않게 이 종목 전용 리스트에만 넣습니다.
            var myBucket = StockDataBuckets[info.Symbol];
            myBucket.Add(ohlc);
            if (myBucket.Count > MaxBars) myBucket.RemoveAt(0);

            // 4. [신호등] 만약 지금 사용자가 '보고 있는' 종목의 데이터가 들어온 것이라면? "화면을 그려라!"
            if (StockList.Count > 0 && StockList[CurrentIndex].Symbol == info.Symbol)
            {
                isDataUpdated = true;
            }
        }

        /// <summary>
        /// 🎨 실제로 현재 선택된 종목의 바구니만 꺼내서 차트판에 그려냅니다.
        /// </summary>
        internal void LoadChart(Flag.JubbyStockInfo info)
        {
            var plt = FormsPlotMain.Plot;
            plt.Clear(); // 화면을 깨끗이 비웁니다.

            // 💡 [선택적 그리기] 현재 보고 있는 종목의 전용 바구니만 가져옵니다.
            if (StockDataBuckets.ContainsKey(info.Symbol))
            {
                var myBucket = StockDataBuckets[info.Symbol];
                if (myBucket.Count > 0)
                {
                    plt.Add.Candlestick(myBucket); // 내 바구니 데이터만 그리기

                    // ✨ [자동 추적] 데이터가 올 때마다 자동으로 줌을 최신화합니다.
                    plt.Axes.AutoScale();

                    // ✨ [에러 수정] XMax 대신 Right를 사용하여 왼쪽 끝(0)을 다시 고정합니다.
                    var limits = plt.Axes.GetLimits();
                    plt.Axes.SetLimitsX(0, Math.Max(10, limits.Right));
                }
            }

            FormsPlotMain.Refresh(); // 최종 렌더링
        }

        #endregion

        #region ## [4] 종목 전환 및 외부 통신 (Interface) ##

        public void ShowStock(string symbol)
        {
            if (StockList == null || StockList.Count == 0) return;
            int idx = StockList.FindIndex(s => s.Symbol == symbol);
            if (idx == -1) return;

            CurrentIndex = idx;
            // 종목을 클릭하자마자 해당 종목의 바구니에 든 데이터를 즉시 보여줍니다.
            LoadChart(StockList[CurrentIndex]);
        }

        internal void SetStockList(List<Flag.JubbyStockInfo> list)
        {
            StockList = list ?? new List<Flag.JubbyStockInfo>();
            CurrentIndex = 0;
            if (StockList.Count > 0) LoadChart(StockList[CurrentIndex]);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!Auto.Ins.DataManager.FirstDataReceived) return;

            if (isDataUpdated)
            {
                LoadChart(StockList[CurrentIndex]);
                isDataUpdated = false; // 신호등 끄기
            }
        }

        private void FormStockChart_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Left) MovePrevStock();
            else if (e.Control && e.KeyCode == Keys.Right) MoveNextStock();
        }

        private void MovePrevStock()
        {
            if (CurrentIndex > 0) { CurrentIndex--; LoadChart(StockList[CurrentIndex]); }
        }

        private void MoveNextStock()
        {
            if (CurrentIndex < StockList.Count - 1) { CurrentIndex++; LoadChart(StockList[CurrentIndex]); }
        }

        #endregion
    }
}