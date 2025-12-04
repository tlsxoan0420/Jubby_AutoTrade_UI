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
        /// 차트 선언
        private FormsPlot FormsPlotMain;

        /// 현재 프로그램에서 관리하는 전체 종목 리스트
        // FormDataChart에서 전달해 줄 예정
        // Ctrl+좌/우 이동 시 여기 있는 리스트 기준으로 이전/다음 종목을 찾는다.
        private List<Flag.JubbyStockInfo> StockList = new List<Flag.JubbyStockInfo>();

        /// StockList 안에서 현재 차트에 표시 중인 종목의 인덱스
        // 0 이상, StockList.Count-1 이하
        private int CurrentIndex = 0;

        // 차트에 그릴 데이터
        /// OHLC 데이터 리스트
        // 캔들(봉) 데이터 저장용 리스트
        // OHLC(시가/고가/저가/종가 + 시간 + 기간)를 담는다.
        // 실제 차트에는 항상 이 리스트 전체를 CandlestickPlot에 넘겨서 그림.
        private readonly List<OHLC> OHLCList = new List<OHLC>();

        /// 거래량 데이터 저장용 리스트
        // 각 봉에 대응하는 거래량 값들을 순서대로 저장.
        // BarPlot.Values에 그대로 전달해서 막대 그래프로 그린다.
        private readonly List<double> OrderHistoryList = new List<double>();

        // ScottPlot 관련 객체 (한 번 생성 후, 데이터만 교체해서 사용)
        private CandlestickPlot CandlePlot;                                        // 캔들 차트용
        private BarPlot VolumePlot;                                                // 거래량용 바 차트
        private Scatter BuyMarkers;                                                // BUY 주문 마커
        private Scatter SellMarkers;                                               // SELL 주문 마커

        /// InitChart()가 이미 실행되었는지 여부
        // 여러 번 초기화하지 않도록 체크용.
        private bool ChartInitialized = false;

        /// 차트에 동시에 표시할 최대 봉 개수
        // 이 개수보다 많아지면 가장 오래된 봉부터 삭제.
        // 너무 많은 봉을 그리면 렌더링이 느려지기 때문에 제한을 둔다.
        private const int MaxBars = 500;

        private bool firstSetDone = false;
        #endregion ## FormGraphic Define ##

        public FormGraphic()
        {
            InitializeComponent();
            UI_Organize();
        }

        #region ## UI Organize ##
        private void UI_Organize()
        {
            ///
            /// 폼에서 키보드 입력 (Ctrl+좌/우)를 받도록 설정
            this.KeyPreview = true;
            this.KeyDown += FormStockChart_KeyDown;

            /// 차트 생성
            FormsPlotMain = new ScottPlot.WinForms.FormsPlot();
            FormsPlotMain.Dock = DockStyle.Fill;    // 패널 또는 폼 전체 꽉 채우기
            this.Controls.Add(FormsPlotMain);       // 폼에 추가

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
            InitChart(); // 기존 차트 초기화 함수 사용 가능
        }
        #endregion ## UI Update ##

        #region ## Graphic Event ##

        #region ## Set Stock List ##
        /// 전체 종목 리스트를 차트 폼에 전달하는 함수.
        // 보통 프로그램 시작 시, 또는 종목 목록이 초기 세팅될 때 한 번 호출.
        // StockList를 받아두고, 첫 번째 종목을 차트에 표시한다.
        internal void SetStockList(List<Flag.JubbyStockInfo> list)
        {
            StockList = list ?? new List<Flag.JubbyStockInfo>();
            CurrentIndex = 0;

            if (StockList.Count > 0)
            {
                LoadChart(StockList[CurrentIndex]);
            }
        }

        /// <summary>
        /// [중요] 외부(FormDataChart 등)에서 특정 종목(symbol)을 선택했을 때,
        ///  해당 심볼을 가진 종목을 _stockList에서 찾아서 차트에 표시하는 함수.
        /// </summary>
        public void ShowStock(string symbol)
        {
            if (StockList == null || StockList.Count == 0)
                return;

            // 심볼로 인덱스 검색
            int idx = StockList.FindIndex(s => s.Symbol == symbol);
            if (idx == -1)
                return; // 못 찾으면 그냥 무시

            CurrentIndex = idx;
            LoadChart(StockList[CurrentIndex]);
        }
        #endregion ## Set Stock List ##

        #region ## Init Chart ##
        /// 차트 세팅 (SccotPlot 객체들을 한 번만 생성)
        private void InitChart()
        {
            var plt = FormsPlotMain.Plot;

            /// X축을 DateTime 모드로 사용
            // → OHLC의 time이 DateTime 기반이라 그래야 자동으로 "시간 축"이 그려짐
            plt.Axes.DateTimeTicksBottom();

            /// 캔들 차트(CandlestickPlot)를 빈 데이터로 먼저 생성
            // 나중에 CandlePlot.Ohlcs 에다가 실제 OHLC 배열만 교체해주면 됨
            CandlePlot = plt.Add.Candlestick(OHLCList.ToArray());

            /// 거래량 BarPlot – 처음엔 값 없는 상태로 생성
            VolumePlot = plt.Add.Bars(Array.Empty<double>());

            /// 스타일 설정 Bars는 배열이기 때문에, 스타일을 통일해주려면 전체를 순회
            foreach (var bar in VolumePlot.Bars)
            {
                // 파란색 + 약간 투명
                bar.FillColor = ScottPlot.Colors.Blue.WithAlpha(0.3);
                // 테두리는 굵기 0
                bar.LineWidth = 0;
            }

            /// [BUY 마커 플롯]
            // 역할 : 매수 시점 표시, 매수 가격 표시 등
            // Scatter: X=시간, Y=가격
            // markerShape만 사용, lineWidth=0으로 선은 없음
            // X/Y 데이터는 나중에 UpdateOrderMarkers()에서 채워 넣음
            BuyMarkers = plt.Add.Scatter(
                xs: Array.Empty<double>(),             // 처음에는 데이터 없음
                ys: Array.Empty<double>(),
                color: ScottPlot.Colors.Lime           // 연두색
            );
            BuyMarkers.MarkerShape = MarkerShape.FilledTriangleDown; // 아래 방향 삼각형
            BuyMarkers.MarkerSize = 10;
            BuyMarkers.LineWidth = 0;

            /// [Sell 마커 플롯]
            // 역할 : 매수 시점 표시, 매수 가격 표시 등
            // Scatter: X=시간, Y=가격
            // markerShape만 사용, lineWidth=0으로 선은 없음
            // X/Y 데이터는 나중에 UpdateOrderMarkers()에서 채워 넣음
            SellMarkers = plt.Add.Scatter(
                xs: Array.Empty<double>(),
                ys: Array.Empty<double>(),
                color: ScottPlot.Colors.Red             // 빨간색
            );
            SellMarkers.MarkerShape = MarkerShape.FilledTriangleUp; // 위 방향 삼각형
            SellMarkers.MarkerSize = 10;
            SellMarkers.LineWidth = 0;

            /// 제목/축 라벨 등 기본 설정
            plt.Title("JUBBY STOCK CHART");
            plt.YLabel("Price");

            ChartInitialized = true;
        }
        #endregion ## Init Chart ##

        #region ## Load Chart ##
        internal void LoadChart(Flag.JubbyStockInfo info)
        {
            // 혹시 InitChart가 안 되었을 경우 대비 (안전장치)
            if (!ChartInitialized)
                InitChart();

            /// 시세 데이터(Market)를 반영해서 "봉 1개" 추가
            // 지금 구조는 "실시간으로 새 봉이 들어오는" 형태 예시다.
            // 나중에 과거 봉들도 그릴 거면 이 부분을 바꾸면 됨.
            AppendOHLCFromMarket(info.Market);

            /// 주문 히스토리를 BUY/SELL 마커로 변환
            // 스레드 충돌 방지를 위해 info.GetOrderListSafe()로 복사본 사용
            UpdateOrderMarkers(info.GetOrderListSafe());

            /// 축 자동 스케일 조정
            // 현재 OHLC + 마커 + 거래량 범위에 맞게 자동으로 확대/축소
            FormsPlotMain.Plot.Axes.AutoScale();

            /// 실제 화면 다시 그리기
            FormsPlotMain.Refresh();
        }
        #endregion ## Load Chart ##

        #region ## Apeend OHLC Form Market ##
        // TradeMarketData 한 건을 받아서
        // OHLC 1개 생성 → _ohlcList에 추가
        // 거래량 1개 → _volumeList에 추가
        // 그리고 CandlestickPlot에 리스트를 반영한다.
        private void AppendOHLCFromMarket(Flag.TradeMarketData m)
        {
            if (m == null)
                return;

            /// 봉의 기준 시간을 지금 시각으로 사용
            // 실제로는 봉 시작 시간(예: 09:01:00)을 넘겨 받아 사용하는 게 더 정확.
            DateTime now = DateTime.Now;

            var ohlc = new OHLC(
                (double)m.Open_Price,
                (double)m.High_Price,
                (double)m.Low_Price,
                (double)m.Last_Price,
                now,                                // startTime
                TimeSpan.FromMinutes(1)             // period
            );

            OHLCList.Add(ohlc);
            if (OHLCList.Count > MaxBars)
                OHLCList.RemoveAt(0);

            /// CandlestickPlot 새로 생성
            CandlePlot = FormsPlotMain.Plot.Add.Candlestick(OHLCList.ToArray());

            /// 거래량 리스트 업데이트
            OrderHistoryList.Add((double)m.Volume);
            if (OrderHistoryList.Count > MaxBars)
                OrderHistoryList.RemoveAt(0);

            /// 기존 VolumePlot 제거
            if (VolumePlot != null)
                FormsPlotMain.Plot.Remove(VolumePlot);

            /// VolumePlot 새로 생성
            VolumePlot = FormsPlotMain.Plot.Add.Bars(OrderHistoryList.ToArray());

            /// Volume 스타일 적용
            foreach (var bar in VolumePlot.Bars)
            {
                bar.FillColor = ScottPlot.Colors.Blue.WithAlpha(0.3);
                bar.LineWidth = 0;
            }
        }
        #endregion ## Apeend OHLC Form Market ##

        #region ## Update Order Markers ##
        // 종목의 주문 리스트(TradeOrderData)를 받아서
        // BUY 주문은 초록색 아래 삼각형
        // SELL 주문은 빨간색 위 삼각형
        private void UpdateOrderMarkers(List<Flag.TradeOrderData> orders)
        {
            var buyX = new List<double>();
            var buyY = new List<double>();
            var sellX = new List<double>();
            var sellY = new List<double>();

            if (orders == null)
                orders = new List<Flag.TradeOrderData>();

            foreach (var o in orders)
            {
                // 문자열 시간 → DateTime 변환
                if (!DateTime.TryParse(o.Order_Time, out DateTime dt))
                    continue;

                double x = dt.ToOADate();          // X축: 시간
                double y = (double)o.Order_Price;  // Y축: 가격

                if (o.Order_Type.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                {
                    buyX.Add(x);
                    buyY.Add(y);
                }
                else if (o.Order_Type.Equals("SELL", StringComparison.OrdinalIgnoreCase))
                {
                    sellX.Add(x);
                    sellY.Add(y);
                }
            }

            /// 기존 BUY 마커 제거
            if (BuyMarkers != null)
                FormsPlotMain.Plot.Remove(BuyMarkers);

            /// BUY 데이터가 하나라도 있으면 새로 생성
            if (buyX.Count > 0)
            {
                BuyMarkers = FormsPlotMain.Plot.Add.Scatter(buyX.ToArray(), buyY.ToArray());

                // BUY 마커 스타일 설정
                BuyMarkers.Color = ScottPlot.Colors.Lime;                  // 초록색
                BuyMarkers.MarkerShape = MarkerShape.FilledTriangleDown;         // 아래 삼각형
                BuyMarkers.MarkerSize = 8;
                BuyMarkers.LineWidth = 0;                                      // 선 없이 점(마커)만
            }
            else
            {
                // BUY 주문이 없다면 마커 객체를 null로 정리해 둠 (옵션)
                BuyMarkers = null;
            }

            /// 기존 SELL 마커 제거
            if (SellMarkers != null)
                FormsPlotMain.Plot.Remove(SellMarkers);

            /// SELL 데이터가 있다면 새로 생성
            if (sellX.Count > 0)
            {
                SellMarkers = FormsPlotMain.Plot.Add.Scatter(sellX.ToArray(), sellY.ToArray());

                // SELL 마커 스타일 설정
                SellMarkers.Color = ScottPlot.Colors.Red;                  // 빨간색
                SellMarkers.MarkerShape = MarkerShape.FilledTriangleUp;          // 위 삼각형
                SellMarkers.MarkerSize = 8;
                SellMarkers.LineWidth = 0;
            }
            else
            {
                SellMarkers = null;
            }
        }
        #endregion ## Update Order Markers ##

        #endregion ## Graphic Event ##

        #region ## UI Event ##
        /// 폼에서 키 입력이 들어왔을 때 호출되는 이벤트 핸들러.
        // Ctrl + ← : 이전 종목
        // Ctrl + → : 다음 종목
        private void FormStockChart_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Left)
            {
                MovePrevStock();
            }
            else if (e.Control && e.KeyCode == Keys.Right)
            {
                MoveNextStock();
            }
        }

        /// 이전 종목으로 이동 (리스트의 앞쪽 인덱스)
        // 이미 0번이면 더 이상 안 넘어감.
        private void MovePrevStock()
        {
            if (StockList == null || StockList.Count == 0)
                return;

            if (CurrentIndex <= 0)
                return; // 가장 앞이면 이동하지 않음

            CurrentIndex--;
            LoadChart(StockList[CurrentIndex]);
        }

        /// 다음 종목으로 이동 (리스트의 뒤쪽 인덱스)
        // 마지막 종목이면 더 이상 안 넘어감.
        private void MoveNextStock()
        {
            if (StockList == null || StockList.Count == 0)
                return;

            if (CurrentIndex >= StockList.Count - 1)
                return; // 마지막이면 이동하지 않음

            CurrentIndex++;
            LoadChart(StockList[CurrentIndex]);
        }

        #endregion ## UI Event ##

        #region ## Timer Event ##
        private void timer1_Tick(object sender, EventArgs e)
        {
            // 아직 데이터가 아예 안 들어옴 → 아무것도 하지 않음
            if (!Auto.Ins.DataManager.FirstDataReceived)
                return;

            // 데이터는 들어왔는데 최초 세팅 안함 → 여기서 최초 1회 실행
            if (!firstSetDone)
            {
                firstSetDone = true;

                string firstSymbol = Auto.Ins.DataManager.FirstSymbol;
                var firstInfo = Auto.Ins.GetStock(firstSymbol);

                if (firstInfo != null)
                {
                    SetStockList(new List<JubbyStockInfo> { firstInfo });
                }

                return; // 여기서 첫 루프 종료
            }

            if (StockList == null || StockList.Count == 0)
                return;

            if (CurrentIndex < 0 || CurrentIndex >= StockList.Count)
                return;

            // 현재 차트에 표시 중인 종목
            string symbol = StockList[CurrentIndex].Symbol;

            // 최신 종목 정보 가져오기
            var info = Auto.Ins.GetStock(symbol);
            if (info == null)
                return;

            // 차트 갱신
            LoadChart(info);
        }
        #endregion ## Timer Event ##
    }
}
