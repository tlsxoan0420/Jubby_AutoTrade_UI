using System;
using System.Collections.Generic;
using System.Data; // 🔥 [추가] DB에서 온 표(DataTable)를 쓰기 위해 필수
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Jubby_AutoTrade_UI.COMMON;
using Jubby_AutoTrade_UI.SEQUENCE; // 🔥 [추가] 지휘관 Auto.cs와 소통하기 위해

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormDataChart : Form
    {
        private DataGridView[] dgvChartArray;

        public FormDataChart()
        {
            InitializeComponent();

            // 🔥 [에러 수정] 생성자 안에서는 UI 초기화(UIOrganize)나 이벤트 구독(SubscribeToEvents)을 하지 않습니다.
            // 대신 화면이 켜질 때 발생하는 Load 이벤트에 이 기능들을 위임합니다.
            this.Load += FormDataChart_Load;
        }

        /// <summary>
        /// 폼 화면이 열릴 준비를 마치고 화면에 뜨기 직전에 실행되는 함수입니다. (Null 참조 방지)
        /// </summary>
        private void FormDataChart_Load(object sender, EventArgs e)
        {
            UIOrganize(); // 1. 표의 모양과 색상을 먼저 잡습니다.
            SubscribeToEvents(); // 2. 지휘관(Auto.cs)의 신호탄을 기다립니다.
        }

        /// <summary>
        /// 📡 [핵심] 지휘관(Auto.cs)이 DB에서 최신 표를 퍼왔을 때 나한테도 알려달라고 등록합니다.
        /// </summary>
        private void SubscribeToEvents()
        {
            // Auto.cs 지휘관이 "데이터 갱신했어!" 라고 외치면 아래 함수들이 즉시 실행됩니다.
            Auto.Ins.OnMarketDataRefreshed += (dt) => UpdateGridDataSource(dgvChart1, dt);
            Auto.Ins.OnAccountDataRefreshed += (dt) => UpdateGridDataSource(dgvChart2, dt);
            Auto.Ins.OnOrderDataRefreshed += (dt) => UpdateGridDataSource(dgvChart3, dt);
            Auto.Ins.OnStrategyDataRefreshed += (dt) => UpdateGridDataSource(dgvChart4, dt);
        }

        /// <summary>
        /// 🚀 [초고속 갱신] DB에서 온 엑셀 표를 화면에 있는 표(그리드)에 0.1초 만에 꽂아 넣습니다.
        /// </summary>
        private void UpdateGridDataSource(DataGridView dgv, DataTable dt)
        {
            // 아직 화면이 생성되지 않았거나, 대상 그리드가 없으면 무시합니다 (에러 방지)
            if (dgv == null || !dgv.IsHandleCreated || this.IsDisposed) return;

            // UI 스레드 충돌을 방지하기 위한 안전장치 (다른 스레드가 화면을 건드리지 못하게 함)
            if (dgv.InvokeRequired)
            {
                dgv.BeginInvoke(new Action(() => UpdateGridDataSource(dgv, dt)));
                return;
            }

            try
            {
                // 선택 상태를 유지하기 위해 현재 몇 번째 줄을 클릭했는지 기억해둡니다.
                int scrollIdx = dgv.FirstDisplayedScrollingRowIndex;
                int selectedIdx = dgv.CurrentRow?.Index ?? -1;

                dgv.DataSource = dt; // 🔥 마법의 한 줄: 표 데이터 전체 즉시 교체

                // 기존에 보던 위치와 선택했던 줄을 복원합니다. (사용자 편의성)
                if (scrollIdx >= 0 && scrollIdx < dgv.RowCount) dgv.FirstDisplayedScrollingRowIndex = scrollIdx;
                if (selectedIdx >= 0 && selectedIdx < dgv.RowCount) dgv.Rows[selectedIdx].Selected = true;

                // 숫자가 예쁘게 보이도록 천단위 콤마(,) 등 서식을 입힙니다.
                ApplyCustomFormatting(dgv);
            }
            catch { /* 찰나의 데이터 꼬임은 무시합니다 */ }
        }

        /// <summary>
        /// ✨ 표에 있는 글자들의 색상이나 숫자의 콤마(,)를 예쁘게 다듬습니다.
        /// </summary>
        private void ApplyCustomFormatting(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.OwningColumn == null) continue;

                    string colName = cell.OwningColumn.Name.ToLower();

                    // 1. 수익률이나 등락률이 플러스면 '라임색', 마이너스면 '하늘색'으로 칠합니다.
                    if (colName.Contains("rate") || colName.Contains("return") || colName.Contains("yield"))
                    {
                        double val = 0;
                        if (double.TryParse(cell.Value?.ToString().Replace("%", ""), out val))
                        {
                            if (val > 0) cell.Style.ForeColor = Color.Lime;
                            else if (val < 0) cell.Style.ForeColor = Color.DeepSkyBlue;
                            else cell.Style.ForeColor = Color.WhiteSmoke;
                        }
                    }
                    // 2. 전략 신호가 BUY면 라임, SELL이면 하늘색
                    if (colName == "signal")
                    {
                        string sig = cell.Value?.ToString().ToUpper();
                        if (sig == "BUY") cell.Style.ForeColor = Color.Lime;
                        else if (sig == "SELL") cell.Style.ForeColor = Color.DeepSkyBlue;
                    }
                }
            }
        }

        // =====================================================================
        // 🎨 UI 초기 셋팅 (표의 색상과 기본 디자인)
        // =====================================================================

        /// <summary>
        /// 주삐 프로젝트의 차트 UI 구성 및 초기 설정을 관리하는 메서드입니다.
        /// </summary>
        public void UIOrganize()
        {
            // 1. DataGridView 컨트롤들을 배열로 묶어서 관리하기 편하게 초기화합니다.
            // (배열 요소인 dgvChart1 등이 null인지 확인하여 안전하게 묶습니다)
            if (dgvChart1 == null || dgvChart2 == null || dgvChart3 == null || dgvChart4 == null) return;

            dgvChartArray = new DataGridView[] { dgvChart1, dgvChart2, dgvChart3, dgvChart4 };

            // 2. 배열에 담긴 차트 개수만큼 반복하며 설정을 적용합니다.
            for (int i = 0; i < dgvChartArray.Length; i++)
            {
                dgvChartArray[i].SelectionChanged -= DgvChart_SelectionChanged;
                dgvChartArray[i].SelectionChanged += DgvChart_SelectionChanged;

                SetGridStyle(dgvChartArray[i]);
                dgvChartArray[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }

        private void SetGridStyle(DataGridView dgv)
        {
            // 💡 [중요] DB 연동 모드에서는 C#이 컬럼을 자동으로 만들게 내버려둡니다.
            dgv.AutoGenerateColumns = true;

            dgv.DoubleBuffered(true); // 깜빡임 방지
            dgv.AllowUserToAddRows = false; dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgv.ReadOnly = true;
            dgv.BorderStyle = BorderStyle.None;
            dgv.BackgroundColor = Color.FromArgb(5, 5, 15); dgv.DefaultCellStyle.BackColor = Color.FromArgb(5, 5, 15);
            dgv.DefaultCellStyle.ForeColor = Color.WhiteSmoke; dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 60);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White; dgv.GridColor = Color.FromArgb(70, 70, 90);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single; dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 20, 35);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.LightGray;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("HY헤드라인M", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 35;
        }

        // 🖱️ 표에서 종목을 클릭했을 때 좌측 차트 화면에 해당 종목을 띄워주는 기능
        private void DgvChart_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = sender as DataGridView;
                if (dgv == null || dgv.CurrentRow == null) return;

                string symbol = "";
                if (dgv.Columns.Contains("symbol")) symbol = dgv.CurrentRow.Cells["symbol"].Value?.ToString();
                else if (dgv.Columns.Contains("Symbol")) symbol = dgv.CurrentRow.Cells["Symbol"].Value?.ToString();

                if (string.IsNullOrWhiteSpace(symbol) || symbol == "-") return;

                var chartForm = Application.OpenForms.OfType<FormGraphic>().FirstOrDefault();
                chartForm?.ShowStock(symbol);
            }
            catch { }
        }
    }

    // [확장 메서드] 표를 그릴 때 버벅이지 않게 해주는 비법 소스
    public static class ExtensionMethods
    {
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            System.Reflection.PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pi?.SetValue(dgv, setting, null); // pi가 null인지 안전하게 확인
        }
    }
}