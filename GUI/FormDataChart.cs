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

            if (dgv.InvokeRequired)
            {
                dgv.BeginInvoke(new Action(() => UpdateGridDataSource(dgv, dt)));
                return;
            }

            try
            {
                // 1. 현재 스크롤 위치와 클릭해둔 종목(Symbol)을 기억합니다.
                int scrollIdx = dgv.FirstDisplayedScrollingRowIndex;
                string selectedSymbol = "";

                if (dgv.CurrentRow != null && dgv.Columns.Contains("Symbol"))
                {
                    selectedSymbol = dgv.CurrentRow.Cells["Symbol"].Value?.ToString();
                }

                // 2. 표가 갱신될 때마다 차트가 깜빡이거나 이벤트가 중복 폭주하는 것을 막기 위해 잠시 끕니다.
                dgv.CellClick -= DgvChart_CellClick;
                dgv.KeyUp -= DgvChart_KeyUp; // 🔥 방향키 이벤트 임시 해제

                // 3. 파이썬이 넘겨준 최신 DB로 표를 덮어씁니다. (무한 누적 X)
                dgv.DataSource = dt.Copy();

                // 4. 아까 기억해둔 스크롤 위치로 되돌립니다.
                if (scrollIdx >= 0 && scrollIdx < dgv.RowCount)
                    dgv.FirstDisplayedScrollingRowIndex = scrollIdx;

                // 5. 아까 선택해뒀던 종목을 다시 파란색으로 칠해주고 포커스를 유지합니다! (마법의 클릭 유지)
                if (!string.IsNullOrEmpty(selectedSymbol))
                {
                    dgv.ClearSelection();
                    foreach (DataGridViewRow r in dgv.Rows)
                    {
                        if (r.Cells["Symbol"].Value?.ToString() == selectedSymbol)
                        {
                            r.Selected = true;
                            dgv.CurrentCell = r.Cells[0]; // 포커스 이동 (스크롤 고정용)
                            break;
                        }
                    }
                }

                // 6. 이벤트들을 다시 켭니다.
                dgv.CellClick += DgvChart_CellClick;
                dgv.KeyUp += DgvChart_KeyUp; // 🔥 방향키 이벤트 재부착
            }
            catch { }
        }

        // =========================================================================
        // 🎨 [마법의 필터] 데이터 원본은 지키면서 화면에만 예쁘게 색칠해주는 이벤트!
        // =========================================================================
        private void DgvChart_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Value == null) return;

            DataGridView dgv = sender as DataGridView;
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
            if (colName == "Pnl_Rate" || colName == "Return_1m" || colName == "Order_Yield")
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
                // 🔥 마우스 클릭과 키보드 이벤트를 모두 부착합니다!
                dgvChartArray[i].CellClick -= DgvChart_CellClick;
                dgvChartArray[i].CellClick += DgvChart_CellClick;

                dgvChartArray[i].KeyUp -= DgvChart_KeyUp;
                dgvChartArray[i].KeyUp += DgvChart_KeyUp;

                // 디자인 포장용 필터 부착
                dgvChartArray[i].CellFormatting -= DgvChart_CellFormatting;
                dgvChartArray[i].CellFormatting += DgvChart_CellFormatting;

                SetGridStyle(dgvChartArray[i], i);
                dgvChartArray[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
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
                AddColumn(dgv, "Time", "시간", 80, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Last_Price", "현재가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Open_Price", "시가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "High_Price", "고가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Low_Price", "저가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Bid_Price", "매수호가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Ask_Price", "매도호가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Bid_Size", "매수잔량", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Ask_Size", "매도잔량", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Return_1m", "1분등락률", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Trade_Amount", "거래대금", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Vol_Energy", "거래량에너지", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Disparity", "이격도", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Volume", "거래량", 60, DataGridViewContentAlignment.MiddleCenter);
            }
            else if (dgv.Columns.Count == 0 && ChartIndex == 1) // 계좌
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Time", "시간", 80, DataGridViewContentAlignment.MiddleCenter);
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
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Time", "주문시간", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Type", "주문종류", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Price", "주문가격", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Quantity", "주문수량", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Filled_Quqntity", "체결수량", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Status", "주문상태", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Yield", "수익률", 60, DataGridViewContentAlignment.MiddleCenter);
            }
            else if (dgv.Columns.Count == 0 && ChartIndex == 3) // 전략
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Time", "시간", 80, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);
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