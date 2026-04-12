using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Jubby_AutoTrade_UI.COMMON;
using Jubby_AutoTrade_UI.SEQUENCE;

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormDataChart : Form
    {
        private DataGridView[] dgvChartArray;
        private bool isUpdatingGrid = false;
        private DataGridView activeDgv = null; // 🔥 [추가] 지금 사용자가 클릭해서 보고 있는 '진짜 주인 표'를 기억합니다.

        public FormDataChart()
        {
            InitializeComponent();
        }

        private void FormDataChart_Load(object sender, EventArgs e)
        {
            UIOrganize();
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            Auto.Ins.OnMarketDataRefreshed += (dt) => UpdateGridDataSource(dgvChart1, dt);
            Auto.Ins.OnAccountDataRefreshed += (dt) => UpdateGridDataSource(dgvChart2, dt);
            Auto.Ins.OnOrderDataRefreshed += (dt) => UpdateGridDataSource(dgvChart3, dt);
            Auto.Ins.OnStrategyDataRefreshed += (dt) => UpdateGridDataSource(dgvChart4, dt);
        }

        private void UpdateGridDataSource(DataGridView dgv, DataTable dt)
        {
            if (dgv == null || !dgv.IsHandleCreated || this.IsDisposed || dt == null) return;
            if (dgv.InvokeRequired) { dgv.BeginInvoke(new Action(() => UpdateGridDataSource(dgv, dt))); return; }

            try
            {
                isUpdatingGrid = true; // 🌟 락 온! (데이터를 교체하는 동안에는 유저 클릭/이동 이벤트를 무시함)

                int scrollIdx = dgv.FirstDisplayedScrollingRowIndex;
                string selectedSymbol = "";
                if (dgv.CurrentRow != null && dgv.Columns.Contains("Symbol")) selectedSymbol = dgv.CurrentRow.Cells["Symbol"].Value?.ToString();

                dgv.CellClick -= DgvChart_CellClick; // (기존 쓰레기 이벤트 안전장치 제거)

                if (dgv.DataSource is DataTable oldDt)
                {
                    oldDt.Dispose();
                }
                dgv.DataSource = dt.Copy();

                if (scrollIdx >= 0 && scrollIdx < dgv.RowCount) dgv.FirstDisplayedScrollingRowIndex = scrollIdx;
                if (!string.IsNullOrEmpty(selectedSymbol))
                {
                    dgv.ClearSelection();
                    foreach (DataGridViewRow r in dgv.Rows)
                    {
                        if (r.Cells["Symbol"].Value?.ToString() == selectedSymbol)
                        {
                            r.Selected = true;
                            dgv.CurrentCell = r.Cells[0];
                            break;
                        }
                    }
                }
            }
            catch { }
            finally
            {
                isUpdatingGrid = false; // 🌟 락 오프! (데이터 교체 완료, 유저 키보드/마우스 조작 다시 접수 시작)
            }
        }

        // =========================================================================
        // 🎨 [마법의 필터] 데이터 원본은 지키면서 화면에만 예쁘게 색칠해주는 이벤트!
        // =========================================================================
        private void DgvChart_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Value == null) return;

            DataGridView dgv = sender as DataGridView;
            // 🚀 컬럼 이름이 "No"이거나 헤더 텍스트가 "번호"인 경우 숫자를 매깁니다.
            if (dgv.Columns[e.ColumnIndex].Name == "No" || dgv.Columns[e.ColumnIndex].HeaderText == "번호")
            {
                e.Value = (e.RowIndex + 1).ToString();
                e.FormattingApplied = true;
                return;
            }

            string colName = dgv.Columns[e.ColumnIndex].Name;
            string valStr = e.Value.ToString();

            // 1. 번호 매기기 (첫 번째 열)
            if (colName == "No")
            {
                e.Value = (e.RowIndex + 1).ToString();
                e.FormattingApplied = true;
                return;
            }

            // 2. 수익률 / 등락률 색상 입히기 및 % 기호 붙이기
            // 🔥 [수정] AI_Prob (상승확률) 도 같이 빨간색으로 예쁘게 칠해지도록 조건에 추가합니다!
            if (colName == "Pnl_Rate" || colName == "Return_1m" || colName == "Order_Yield" || colName == "AI_Prob")
            {
                if (double.TryParse(valStr.Replace("%", "").Replace(",", ""), out double val))
                {
                    if (val > 0) e.CellStyle.ForeColor = Color.Red;
                    else if (val < 0) e.CellStyle.ForeColor = Color.DeepSkyBlue;
                    else e.CellStyle.ForeColor = Color.WhiteSmoke;

                    e.Value = $"{val:F2}%";
                    e.FormattingApplied = true;
                }
            }
            // 3. 주문 종류를 한글로 완벽 번역
            else if (colName == "Order_Type" || colName == "Status" || colName == "Signal")
            {
                string upperVal = valStr.ToUpper();
                if (upperVal.Contains("SELL_PROFIT")) { e.Value = "익절"; e.CellStyle.ForeColor = Color.DeepSkyBlue; }
                else if (upperVal.Contains("SELL_LOSS")) { e.Value = "손절"; e.CellStyle.ForeColor = Color.DeepSkyBlue; }
                else if (upperVal.Contains("SELL") || upperVal.Contains("매도")) { e.Value = "매도"; e.CellStyle.ForeColor = Color.DeepSkyBlue; }
                else if (upperVal.Contains("BUY") || upperVal.Contains("매수") || upperVal.Contains("ADD")) { e.Value = "매수"; e.CellStyle.ForeColor = Color.Red; }
                else if (upperVal.Contains("WAIT")) { e.Value = "대기"; e.CellStyle.ForeColor = Color.Yellow; }
                e.FormattingApplied = true;
            }
            // 4. 숫자에 천단위 콤마(,) 렌더링
            else if (colName.Contains("Price") || colName.Contains("Quantity") || colName.Contains("Amount") || colName.Contains("Pnl") || colName.Contains("Cash") || colName.Contains("Size") || colName == "Ma_5" || colName == "Ma_20" || colName == "Volume")
            {
                string rawValue = valStr.Replace(",", "");
                if (double.TryParse(rawValue, out double numVal))
                {
                    e.Value = numVal.ToString("N0");
                    e.FormattingApplied = true;
                }
            }
        }

        public void UIOrganize()
        {
            if (dgvChart1 == null || dgvChart2 == null || dgvChart3 == null || dgvChart4 == null) return;
            dgvChartArray = new DataGridView[] { dgvChart1, dgvChart2, dgvChart3, dgvChart4 };

            for (int i = 0; i < dgvChartArray.Length; i++)
            {
                // 🔥 사용자가 직접 마우스로 누른 표가 '차트의 주인'이 되도록 설정합니다.
                dgvChartArray[i].MouseDown += (s, e) => { activeDgv = s as DataGridView; };
                dgvChartArray[i].Enter += (s, e) => { activeDgv = s as DataGridView; };

                dgvChartArray[i].SelectionChanged += DgvChart_SelectionChanged;
                dgvChartArray[i].SelectionChanged += DgvChart_SelectionChanged;

                dgvChartArray[i].CellFormatting -= DgvChart_CellFormatting;
                dgvChartArray[i].CellFormatting += DgvChart_CellFormatting;

                SetGridStyle(dgvChartArray[i], i);
                dgvChartArray[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }

            if (dgvChartArray.Length > 0) activeDgv = dgvChartArray[0];
        }

        private void DgvChart_SelectionChanged(object sender, EventArgs e)
        {
            if (isUpdatingGrid) return;

            // 🔥 내가 지금 보고 있는(클릭한) 표가 보낸 신호가 아니면 차트 변경을 무시합니다!
            if (sender != activeDgv) return;

            UpdateChartFromSelectedRow(sender as DataGridView);
        }

        private void SetGridStyle(DataGridView dgv, int ChartIndex)
        {
            dgv.AutoGenerateColumns = false;
            dgv.DoubleBuffered(true); dgv.AllowUserToAddRows = false; dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgv.ReadOnly = true;
            dgv.BorderStyle = BorderStyle.None; dgv.BackgroundColor = Color.FromArgb(5, 5, 15);
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(5, 5, 15); dgv.DefaultCellStyle.ForeColor = Color.WhiteSmoke;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 60); dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.GridColor = Color.FromArgb(70, 70, 90); dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dgv.EnableHeadersVisualStyles = false; dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 20, 35);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.LightGray;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("HY헤드라인M", 11, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 35;
            dgv.DataError += (s, e) => { e.ThrowException = false; };

            if (dgv.Columns.Count == 0 && ChartIndex == 0) // 마켓
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Last_Price", "현재가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Open_Price", "시가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "High_Price", "고가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Low_Price", "저가", 60, DataGridViewContentAlignment.MiddleCenter);
                // 파이썬이 안 주는 호가/잔량 컬럼들은 삭제했습니다 (빈칸 방지)
                AddColumn(dgv, "Return_1m", "1분등락률", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Trade_Amount", "거래대금", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Vol_Energy", "거래량에너지", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Disparity", "이격도", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Volume", "거래량", 60, DataGridViewContentAlignment.MiddleCenter);
            }
            else if (dgv.Columns.Count == 0 && ChartIndex == 1) // 계좌
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);
                // 🔥 [수정] DB에 없는 Time 컬럼 삭제
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Quantity", "보유수량", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Avg_Price", "평균 매입가", 80, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Current_Price", "현재가", 80, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Pnl", "평가손익", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Pnl_Rate", "수익률", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Available_Cash", "주문가능 금액", 80, DataGridViewContentAlignment.MiddleCenter);
            }
            else if (dgv.Columns.Count == 0 && ChartIndex == 2) // 주문
            {
                // (이 부분은 수정 없이 그대로 유지)
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Time", "주문시간", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Type", "주문종류", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Price", "주문가격", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Quantity", "주문수량", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Filled_Quantity", "체결수량", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Status", "주문상태", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Yield", "수익률", 60, DataGridViewContentAlignment.MiddleCenter);
            }
            else if (dgv.Columns.Count == 0 && ChartIndex == 3) // 전략
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);
                // 🔥 [수정] DB에 없는 Time 컬럼 삭제하고 누락된 상승확률 추가
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "AI_Prob", "상승확률", 60, DataGridViewContentAlignment.MiddleCenter); // 💡 누락 복구!
                AddColumn(dgv, "Ma_5", "단기 이동평균", 80, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Ma_20", "장기 이동평균", 80, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "RSI", "RSI 지표", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "MACD", "MACD 지표", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Signal", "전략 신호", 60, DataGridViewContentAlignment.MiddleCenter);
            }
        }

        private void AddColumn(DataGridView dgv, string dataPropertyName, string headerText, int width, DataGridViewContentAlignment align)
        {
            int idx = dgv.Columns.Add(dataPropertyName, headerText);
            dgv.Columns[idx].DataPropertyName = dataPropertyName;
            dgv.Columns[idx].Width = width;
            dgv.Columns[idx].DefaultCellStyle.Alignment = align;
        }

        // =========================================================================
        // 🎯 [공통 함수] 마우스나 키보드로 선택된 줄의 차트를 그려줍니다.
        // =========================================================================
        private void UpdateChartFromSelectedRow(DataGridView dgv)
        {
            try
            {
                if (dgv == null || !dgv.Columns.Contains("Symbol")) return;
                if (dgv.CurrentRow == null || dgv.CurrentRow.Index < 0) return;

                // 선택된 줄에서 'Symbol(종목코드)'을 추출합니다.
                string symbol = dgv.CurrentRow.Cells["Symbol"].Value?.ToString();

                if (string.IsNullOrWhiteSpace(symbol) || symbol == "-") return;

                // 🔥 시스템 심장부(Auto.Ins)에 이미 열려있는 차트 창(formGraphic)을 다이렉트로 호출하여 차트를 바꿉니다!
                Auto.Ins.formGraphic?.ShowStock(symbol);
            }
            catch { }
        }

        // =========================================================================
        // 🖱️ [마우스 이벤트]
        // =========================================================================
        private void DgvChart_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // 헤더 부분(제목) 클릭은 무시
            UpdateChartFromSelectedRow(sender as DataGridView);
        }

        // =========================================================================
        // ⌨️ [키보드 이벤트]
        // =========================================================================
        private void DgvChart_KeyUp(object sender, KeyEventArgs e)
        {
            // 사용자가 위(Up) 또는 아래(Down) 방향키를 눌렀을 때만 차트 업데이트!
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                UpdateChartFromSelectedRow(sender as DataGridView);
            }
        }
    }

    public static class ExtensionMethods
    {
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            System.Reflection.PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pi?.SetValue(dgv, setting, null);
        }
    }
}