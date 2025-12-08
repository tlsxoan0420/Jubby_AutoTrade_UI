using Jubby_AutoTrade_UI.COMMON;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Jubby_AutoTrade_UI.COMMON.Flag;

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormDataChart : Form
    {
        #region ## FormDataChart Define ##
        private DataGridView[] dgvChartArray;

        /// [핵심] 4개의 표 각각을 위한 검색용 지도(Dictionary) 배열
        // _rowMaps[0]은 1번 표의 지도, _rowMaps[1]은 2번 표의 지도
        private Dictionary<string, DataGridViewRow>[] ChartrowMaps;

        /// 선택된 종목을 보여줄 차트 폼에 대한 참조
        // 외부에서 생성한 FormStockChart를 받아서 여기에 보관해 둔다.
        private FormGraphic formGraphic;
        #endregion ## FormDataChart Define ##

        public FormDataChart()
        {
            InitializeComponent();
            UIOrganize();
        }

        #region ## UI Organize ##
        public void UIOrganize()
        {
            dgvChartArray = new DataGridView[]
            {
                dgvChart1,
                dgvChart2,
                dgvChart3,
                dgvChart4,
            };

            // 딕셔너리 배열도 4칸 생성
            ChartrowMaps = new Dictionary<string, DataGridViewRow>[4];

            for (int i = 0; i < dgvChartArray.Length; i++)
            {
                // 각 표마다 새로운 딕셔너리 생성
                ChartrowMaps[i] = new Dictionary<string, DataGridViewRow>();

                dgvChartArray[i].CellClick -= DgvChart_CellClick;
                dgvChartArray[i].CellClick += DgvChart_CellClick;
            }

            for (int i = 0; i < dgvChartArray.Length; i++)
            {
                SetGridStyle(dgvChartArray[i], i);
                dgvChartArray[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }
        #endregion ## UI Organize ##

        #region ## FormDataChart Load ##
        private void FormDataChart_Load(object sender, EventArgs e)
        {
            UIUpdate();
        }
        #endregion ## FormDataChart Load ##

        #region ## UI Update ##
        public void UIUpdate()
        {

        }
        #endregion ## UI Update ##

        #region ## Chart Event ##

        #region ## Set Grid Style ##
        private void SetGridStyle(DataGridView dgv, int Chart)
        {
            // 1. 성능 및 기본 설정
            dgv.DoubleBuffered(true); // 깜빡임 방지 (확장메서드 활용)
            dgv.AllowUserToAddRows = false; // 마지막 빈 행 제거
            dgv.RowHeadersVisible = false; // 행 헤더 숨기기
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // 행 전체 선택
            dgv.ReadOnly = true; // 읽기 전용
            dgv.BorderStyle = BorderStyle.None; // 테두리 없음


            // 2. [다크모드 색상] (5, 5, 15) 테마 적용
            dgv.BackgroundColor = Color.FromArgb(5, 5, 15); // 빈 공간 배경

            // 셀 스타일
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(5, 5, 15);
            dgv.DefaultCellStyle.ForeColor = Color.WhiteSmoke;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 60);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            // 그리드 선 색상 (밝은 청회색)
            dgv.GridColor = Color.FromArgb(70, 70, 90);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            // 3. 헤더 스타일
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 20, 35);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.LightGray;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("HY헤드라인M", 11, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 35;

            // 4. 컬럼 추가
            // (이미 컬럼이 있다면 중복 추가 방지)
            if (dgv.Columns.Count == 0 && Chart == 0) // 1. 시세 정보 데이터
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);              // 0. 번호
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);      // 1. 종목코드
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);          // 2. 종목명
                AddColumn(dgv, "Last_Price", "현재가", 60, DataGridViewContentAlignment.MiddleCenter);    // 3. 현재가
                AddColumn(dgv, "Open_Price", "시가", 60, DataGridViewContentAlignment.MiddleCenter);      // 4. 시가
                AddColumn(dgv, "High_Price", "고가", 60, DataGridViewContentAlignment.MiddleCenter);      // 5. 고가
                AddColumn(dgv, "Low_Price", "저가", 60, DataGridViewContentAlignment.MiddleCenter);       // 6. 저가
                AddColumn(dgv, "Bid_Price", "매수호가", 60, DataGridViewContentAlignment.MiddleCenter);   // 7. 매수호가
                AddColumn(dgv, "Ask_Price", "매도호가", 60, DataGridViewContentAlignment.MiddleCenter);   // 8. 매도호가
                AddColumn(dgv, "Bid_Size", "매수잔량", 60, DataGridViewContentAlignment.MiddleCenter);    // 9. 매수잔량
                AddColumn(dgv, "Ask_Size", "매도잔량", 60, DataGridViewContentAlignment.MiddleCenter);    // 10. 매도잔량
                AddColumn(dgv, "Volume", "거래량", 60, DataGridViewContentAlignment.MiddleCenter);        // 11. 거래량

            }
            else if (dgv.Columns.Count == 0 && Chart == 1) // 2. 내 잔고 정보 데이터
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);                        // 0. 번호
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);                // 1. 종목코드
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);                    // 2. 종목명
                AddColumn(dgv, "Quantity", "보유수량", 60, DataGridViewContentAlignment.MiddleCenter);              // 3. 보유수량
                AddColumn(dgv, "Avg_Price", "평균 매입가", 80, DataGridViewContentAlignment.MiddleCenter);          // 4. 평균 매입가
                AddColumn(dgv, "Pnl", "평가손익", 60, DataGridViewContentAlignment.MiddleCenter);                   // 5. 평가손익
                AddColumn(dgv, "Available_Cash", "주문가능 금액", 80, DataGridViewContentAlignment.MiddleCenter);   // 6. 주문 가능 금액

            }
            else if (dgv.Columns.Count == 0 && Chart == 2) // 3. 전략 분석 정보 데이터
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);                  // 0. 번호
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);          // 1. 종목코드
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);              // 2. 종목명
                AddColumn(dgv, "Order_Type", "주문종류", 60, DataGridViewContentAlignment.MiddleCenter);     // 3. 주문종류
                AddColumn(dgv, "Order_Price", "주문가격", 60, DataGridViewContentAlignment.MiddleCenter);     // 4. 주문가격
                AddColumn(dgv, "Order_Quantity", "주문수량", 60, DataGridViewContentAlignment.MiddleCenter);  // 5. 주문수량
                AddColumn(dgv, "Filled_Quqntity", "체결수량", 60, DataGridViewContentAlignment.MiddleCenter); // 6. 체결수량
                AddColumn(dgv, "Order_Time", "주문시간", 60, DataGridViewContentAlignment.MiddleCenter);      // 7. 주문시간
                AddColumn(dgv, "Status", "주문상태", 60, DataGridViewContentAlignment.MiddleCenter);          // 8. 주문상태

            }
            else if (dgv.Columns.Count == 0 && Chart == 3) // 4. 주문내역 데이터
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);                  // 0. 번호
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);          // 1. 종목코드
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);              // 2. 종목명
                AddColumn(dgv, "Ma_5", "단기 이동평균", 80, DataGridViewContentAlignment.MiddleCenter);       // 3. 단기 이동평균
                AddColumn(dgv, "Ma_20", "장기 이동평균", 80, DataGridViewContentAlignment.MiddleCenter);      // 4. 장기 이동평균
                AddColumn(dgv, "RSI", "RSI 지표", 60, DataGridViewContentAlignment.MiddleCenter);             // 5. RSI 지표
                AddColumn(dgv, "MACD", "MACD 지표", 60, DataGridViewContentAlignment.MiddleCenter);           // 6. MACD 지표
                AddColumn(dgv, "Signal", "전략 신호", 60, DataGridViewContentAlignment.MiddleCenter);         // 7. 전략 신호 (매수 / 매도 / NONE)
            }

            // 컬럼 자동 채우기
            //dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // 컬럼 추가 헬퍼 (어느 그리드에 추가할지 dgv 파라미터 받음)
        private void AddColumn(DataGridView dgv, string name, string header, int width, DataGridViewContentAlignment align)
        {
            int idx = dgv.Columns.Add(name, header);
            dgv.Columns[idx].Width = width;
            dgv.Columns[idx].DefaultCellStyle.Alignment = align;
        }
        #endregion ## Set Grid Style ##

        #region ## Update Chart Data ##
        private void UpdateData(int targetIndex, Flag.JubbyStockInfo stock, Flag.UpdateTarget target)
        {
            // 1. [안전장치] 인덱스가 범위를 벗어나면 무시
            if (targetIndex < 0 || targetIndex >= dgvChartArray.Length) return;

            DataGridView targetGrid = dgvChartArray[targetIndex];
            Dictionary<string, DataGridViewRow> targetMap = ChartrowMaps[targetIndex];

            // 2. [UI 스레드 처리] 다른 스레드에서 호출 시 충돌 방지
            if (targetGrid.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateData(targetIndex, stock, target)));
                return;
            }

            // 3. 데이터 갱신 로직
            if (targetMap.ContainsKey(stock.Symbol))
            {
                // [A] 이미 있는 종목 -> 값만 변경 (업데이트)
                DataGridViewRow row = targetMap[stock.Symbol];
                SetRowValues(row, stock, target);
                targetGrid.Refresh();
            }
            else
            {
                // [B] 없는 종목 -> 새로 추가
                int rowIndex = targetGrid.Rows.Add();
                DataGridViewRow row = targetGrid.Rows[rowIndex];

                // 원본 객체를 Tag에 저장해 두면 나중에 삭제/수정 시 유용
                row.Tag = stock;

                // 검색용 지도에 등록
                targetMap.Add(stock.Symbol, row);

                // 불변 데이터 세팅 (순위, 코드, 이름)
                row.Cells["No"].Value = rowIndex + 1; // 순위 (1부터 시작)
                row.Cells["Symbol"].Value = stock.Symbol;
                row.Cells["Name"].Value = stock.Name;

                foreach (DataGridViewColumn col in row.DataGridView.Columns)
                {
                    Debug.WriteLine($"COL: {col.Name}");
                }
                SetRowValues(row, stock, target);
                Debug.WriteLine($"RowIndex = {row.Index}, Symbol = {stock.Symbol}");
                targetGrid.Refresh();
            }
        }

        // [공통] 행에 값 채워넣는 함수 (코드 중복 방지)
        // target 파라미터 추가!
        private void SetRowValues(DataGridViewRow row, Flag.JubbyStockInfo stock, Flag.UpdateTarget target)
        {
            // 1. [Market] 시세 데이터 업데이트
            if (target == Flag.UpdateTarget.All || target == Flag.UpdateTarget.Market)
            {
                row.Cells["Last_Price"].Value = stock.Market.Last_Price.ToString("N6");     // 1. 현재가
                row.Cells["Open_Price"].Value = stock.Market.Open_Price.ToString("N6");     // 2. 시가
                row.Cells["High_Price"].Value = stock.Market.High_Price.ToString("N6");     // 3. 고가
                row.Cells["Low_Price"].Value = stock.Market.Low_Price.ToString("N6");       // 4. 저가
                row.Cells["Bid_Price"].Value = stock.Market.Bid_Price.ToString("N6");       // 5. 매수호가
                row.Cells["Ask_Price"].Value = stock.Market.Ask_Price.ToString("N6");       // 6. 매도호가
                row.Cells["Bid_Size"].Value = stock.Market.Bid_Size.ToString("N6");         // 7. 매수잔량
                row.Cells["Ask_Size"].Value = stock.Market.Ask_Size.ToString("N6");         // 8. 매도잔량
                row.Cells["Volume"].Value = stock.Market.Volume.ToString("N6");             // 9. 거래량

                // 금액에 따른 색상 처리 추가 예정
                //Color color = stock.Market.Change_Rate > 0 ? Color.Red :
                //              (stock.Market.Change_Rate < 0 ? Color.Blue : Color.WhiteSmoke);
                //row.Cells["Price"].Style.ForeColor = color;
                //row.Cells["Rate"].Style.ForeColor = color;
            }

            // 2. [Account] 잔고 데이터 업데이트
            if (target == Flag.UpdateTarget.All || target == Flag.UpdateTarget.Account)
            {
                // 시세가 1초에 10번 변해도, 잔고는 안 변했으면 이 코드는 실행 안 됨 (성능 이득)
                row.Cells["Quantity"].Value = stock.MyAccount.Quantity.ToString("N6");              // 1. 보유수량
                row.Cells["Avg_Price"].Value = stock.MyAccount.Avg_Price.ToString("N6");            // 2. 평균 매입가
                row.Cells["Pnl"].Value = stock.MyAccount.Pnl.ToString("N6");                        // 3. 평가손익
                row.Cells["Available_Cash"].Value = stock.MyAccount.Available_Cash.ToString("N6");  // 4. 주문 가능 금액
            }

            // 3. [Strategy] 전략 분석 정보 데이터 업데이트
            if (target == Flag.UpdateTarget.All || target == Flag.UpdateTarget.Order)
            {
                // 리스트가 존재하고 데이터가 하나라도 있어야 함
                // OrderHistory는 List이므로 바로 .Status 접근 불가 -> 가장 최근 것(.Last) 하나만 가져옴
                var lastOrder = stock.GetOrderListSafe().LastOrDefault(); // 아까 만든 Safe 함수 활용

                if (lastOrder != null)
                {
                    row.Cells["Order_Type"].Value = lastOrder.Order_Type;                               // 1. 주문종류
                    row.Cells["Order_Price"].Value = lastOrder.Order_Price.ToString("N6");              // 2. 주문가격
                    row.Cells["Order_Quantity"].Value = lastOrder.Order_Quantity.ToString("N6");        // 3. 주문수량
                    row.Cells["Filled_Quqntity"].Value = lastOrder.Filled_Quqntity.ToString("N6");      // 4. 체결수량
                    row.Cells["Order_Time"].Value = lastOrder.Order_Time;                               // 5. 주문시간
                    row.Cells["Status"].Value = lastOrder.Status;                                       // 6. 주문상태

                    // 주문 상태별 색상 (예시)
                    // if (lastOrder.Status == "체결") row.Cells["Order_Status"].Style.ForeColor = Color.Red;
                }
            }


            // 4. [Order History] 주문 내역 데이터 업데이트
            if (target == Flag.UpdateTarget.All || target == Flag.UpdateTarget.Strategy)
            {
                if (stock.Strategy != null)
                {
                    row.Cells["Ma_5"].Value = stock.Strategy.Ma_5.ToString("N6");                   // 1. 단기 이동평균
                    row.Cells["Ma_20"].Value = stock.Strategy.Ma_20.ToString("N6");                 // 2. 장기 이동평균
                    row.Cells["RSI"].Value = stock.Strategy.RSI.ToString("N6");                     // 3. RSI 지표
                    row.Cells["MACD"].Value = stock.Strategy.MACD.ToString("N6");                   // 4. MACD 지표
                    row.Cells["Signal"].Value = stock.Strategy.Signal;                              // 5. 전략 신호

                    // 신호 배경색 처리 로직...
                    //if (stock.Strategy.Signal == "BUY")
                    //    row.Cells["Signal"].Style.BackColor = Color.Gold;
                }
            }
        }
        #endregion ## Update Chart Data ##

        #region ## Remove Chart Data ##
        // [데이터 삭제 함수]
        // targetIndex: 몇 번째 표에서 지울지 (0~3)
        // symbol: 지울 종목 코드 (예: "005930")
        private void RemoveData(int targetIndex, string symbol, Flag.UpdateTarget target)
        {
            // 1. [안전장치] 인덱스 범위 확인
            if (targetIndex < 0 || targetIndex >= dgvChartArray.Length) return;

            DataGridView targetGrid = dgvChartArray[targetIndex];
            Dictionary<string, DataGridViewRow> targetMap = ChartrowMaps[targetIndex];

            // 2. [UI 스레드 보호] 다른 스레드(통신)에서 호출 시 충돌 방지
            if (targetGrid.InvokeRequired)
            {
                this.Invoke(new Action(() => RemoveData(targetIndex, symbol, target)));
                return;
            }

            // 3. 삭제 로직 수행
            if (targetMap.ContainsKey(symbol))
            {
                // (1) 지울 행(Row) 객체 찾기
                DataGridViewRow row = targetMap[symbol];

                // (2) 화면(Grid)에서 제거
                targetGrid.Rows.Remove(row);

                // (3) 검색 지도(Dictionary)에서도 제거 (★중요: 이거 안 하면 나중에 에러 남)
                targetMap.Remove(symbol);
                // [조건] 타겟이 '주문내역(OrderHistory)' 삭제인 경우
                if (target == Flag.UpdateTarget.Order)
                {
                    // 1. 해당 종목이 존재하는지 확인 (몇 번째 표인지 targetIndex 필요)
                    if (ChartrowMaps[targetIndex].ContainsKey(symbol))
                    {
                        row = ChartrowMaps[targetIndex][symbol];

                        // 2. 행(Row)의 뒷주머니(Tag)에서 원본 객체(stock) 꺼내기
                        if (row.Tag is Flag.JubbyStockInfo stock)
                        {
                            // 3. 실제 객체에게 삭제 명령 (클래스명X -SetGridStyle> 변수명O)
                            // ★주의: RemoveOrder 안에는 "주문번호(ID)"가 들어가야 합니다.
                            // (만약 파라미터 symbol에 주문번호를 담아 오셨다면 이대로 쓰세요)
                            stock.RemoveOrder(symbol);
                        }
                    }
                }

                // (4) 순번(No) 재정렬 (중간이 빠졌으니 1, 2, 3... 다시 매김)
                ReindexRows(targetGrid);
            }
        }

        // [순번 재정렬 헬퍼 함수]
        // 중간에 2번이 삭제되면 3번->2번, 4번->3번으로 당겨줌
        private void ReindexRows(DataGridView dgv)
        {
            // 표에 남아있는 모든 줄을 훑으면서 번호를 다시 매김
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                // 행 인덱스(0부터 시작)에 +1을 해서 "No" 컬럼에 넣음
                dgv.Rows[i].Cells["No"].Value = i + 1;
            }
        }
        #endregion ## Remove Chart Data ##

        #region ## Apply Stock Update ##
        internal void ApplyStockUpdate(JubbyStockInfo stock, UpdateTarget target)
        {
            // UI 스레드에서 실행
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ApplyStockUpdate(stock, target)));
                return;
            }

            // Market 데이터 → 표 0
            if (target == UpdateTarget.Market || target == UpdateTarget.All)
                UpdateData(0, stock, UpdateTarget.Market);

            // Account 데이터 → 표 1
            if (target == UpdateTarget.Account || target == UpdateTarget.All)
                UpdateData(1, stock, UpdateTarget.Account);

            // OrderHistory → 표 2
            if (target == UpdateTarget.Order || target == UpdateTarget.All)
                UpdateData(2, stock, UpdateTarget.Order);

            // Strategy → 표 3
            if (target == UpdateTarget.Strategy || target == UpdateTarget.All)
                UpdateData(3, stock, UpdateTarget.Strategy);
        }

        #endregion ## Apply Stock Update ##

        internal void SafeApplyStockUpdate(JubbyStockInfo stock, UpdateTarget target)
        {
            if (this.IsDisposed) return; // 폼이 이미 닫혔으면 무시

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => ApplyStockUpdate(stock, target)));
            }
            else
            {
                ApplyStockUpdate(stock, target);
            }
        }

        #endregion ## Chart Event ##

        #region ## UI Event ##
        /// 4개의 Chart Grid 중 어디를 클릭해도 공통으로 들어오는 이벤트 핸들러.
        // 클릭된 행의 Symbol 값을 읽어서
        // 차트 폼(FormStockChart)에 "이 종목 보여줘" 요청을 보낸다.
        private void DgvChart_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // 헤더 클릭은 무시
                if (e.RowIndex < 0)
                    return;

                var dgv = sender as DataGridView;
                if (dgv == null)
                    return;

                // Symbol 컬럼이 없으면 아무 것도 안 함
                if (!dgv.Columns.Contains("Symbol"))
                    return;

                DataGridViewRow row = dgv.Rows[e.RowIndex];

                // Symbol 값 가져오기
                string symbol = Convert.ToString(row.Cells["Symbol"].Value);

                if (string.IsNullOrWhiteSpace(symbol))
                    return;

                // 차트 폼이 설정되어 있으면 해당 종목 차트로 이동
                formGraphic?.ShowStock(symbol);
            }
            catch
            {

            }
        }
        #endregion ## UI Event ##

        #region ## Timer Event ##
        private void timer1_Tick(object sender, EventArgs e)
        {

        }
        #endregion ## Timer Event ##

        private void dgvChart1_DoubleClick(object sender, EventArgs e)
        {
            Random rnd = new Random();
            var stock = new JubbyStockInfo(rnd.NextDouble().ToString(), "TestData");

            stock.Market.Last_Price = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Open_Price = (decimal)rnd.NextDouble() * 1000;
            stock.Market.High_Price = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Low_Price = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Bid_Price = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Ask_Price = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Bid_Size = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Ask_Size = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Volume = (decimal)rnd.NextDouble() * 1000;
            UpdateData(0, stock, Flag.UpdateTarget.Market);
        }
    }

    #region ## DataGridView Extension Method 깜빡임 방지 확장 메서드 ##
    public static class ExtensionMethods
    {
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            System.Reflection.PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }
    }
    #endregion ## DataGridView Extension Method 깜빡임 방지 확장 메서드 ##
}
