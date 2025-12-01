using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormDataChart : Form
    {
        #region ## FormDataChart Define ##
        private DataGridView[] dgvChartArray;

        // [핵심] 4개의 표 각각을 위한 검색용 지도(Dictionary) 배열
        // _rowMaps[0]은 1번 표의 지도, _rowMaps[1]은 2번 표의 지도...
        private Dictionary<string, DataGridViewRow>[] ChartrowMaps;
        #endregion ## FormDataChart Define ##
        public FormDataChart()
        {
            InitializeComponent();
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
            for(int i =0; i < dgvChartArray.Length; i++)
            {
                SetGridStyle(dgvChartArray[i],i);
            }
        }
        #endregion ## UI Update ##

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
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("HY헤드라인M", 15, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 35;

            // 4. 컬럼 추가
            // (이미 컬럼이 있다면 중복 추가 방지)
            if (dgv.Columns.Count == 0 && Chart == 0) // 1. 시세 정보 데이터
            {
                AddColumn(dgv, "NO", "번호", 40, DataGridViewContentAlignment.MiddleCenter);              // 0. 번호
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
                AddColumn(dgv, "NO", "번호", 40, DataGridViewContentAlignment.MiddleCenter);                        // 0. 번호
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);                // 1. 종목코드
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);                    // 2. 종목명
                AddColumn(dgv, "Quantity", "보유수량", 60, DataGridViewContentAlignment.MiddleCenter);              // 3. 보유수량
                AddColumn(dgv, "Avg_Price", "평균 매입가", 80, DataGridViewContentAlignment.MiddleCenter);          // 4. 평균 매입가
                AddColumn(dgv, "Pnl", "평가손익", 60, DataGridViewContentAlignment.MiddleCenter);                   // 5. 평가손익
                AddColumn(dgv, "Available_Cash", "주문가능 금액", 80, DataGridViewContentAlignment.MiddleCenter);   // 6. 주문 가능 금액

            }
            else if (dgv.Columns.Count == 0 && Chart == 2) // 3. 전략 분석 정보 데이터
            {
                AddColumn(dgv, "NO", "번호", 40, DataGridViewContentAlignment.MiddleCenter);                  // 0. 번호
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);          // 1. 종목코드
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);              // 2. 종목명
                AddColumn(dgv, "Order_Trype", "주문종류", 60, DataGridViewContentAlignment.MiddleCenter);     // 3. 주문종류
                AddColumn(dgv, "Order_Price", "주문가격", 60, DataGridViewContentAlignment.MiddleCenter);     // 4. 주문가격
                AddColumn(dgv, "Order_Quantity", "주문수량", 60, DataGridViewContentAlignment.MiddleCenter);  // 5. 주문수량
                AddColumn(dgv, "Filled_Quqntity", "체결수량", 60, DataGridViewContentAlignment.MiddleCenter); // 6. 체결수량
                AddColumn(dgv, "Order_Time", "주문시간", 60, DataGridViewContentAlignment.MiddleCenter);      // 7. 주문시간
                AddColumn(dgv, "Status", "주문상태", 60, DataGridViewContentAlignment.MiddleCenter);          // 8. 주문상태

            }
            else if (dgv.Columns.Count == 0 && Chart == 3) // 4. 주문내역 데이터
            {
                AddColumn(dgv, "NO", "번호", 40, DataGridViewContentAlignment.MiddleCenter);                  // 0. 번호
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);          // 1. 종목코드
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);              // 2. 종목명
                AddColumn(dgv, "Ma_5", "단기 이동평균", 80, DataGridViewContentAlignment.MiddleCenter);       // 3. 단기 이동평균
                AddColumn(dgv, "Ma_20", "장기 이동평균", 80, DataGridViewContentAlignment.MiddleCenter);      // 4. 장기 이동평균
                AddColumn(dgv, "RIS", "RSI 지표", 60, DataGridViewContentAlignment.MiddleCenter);             // 5. RSI 지표
                AddColumn(dgv, "MACD", "MACD 지표", 60, DataGridViewContentAlignment.MiddleCenter);           // 6. MACD 지표
                AddColumn(dgv, "Signal", "전략 신호", 60, DataGridViewContentAlignment.MiddleCenter);         // 7. 전략 신호 (매수 / 매도 / NONE)
            }

            // 컬럼 자동 채우기
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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

        #endregion ## Update Chart Data ##
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
