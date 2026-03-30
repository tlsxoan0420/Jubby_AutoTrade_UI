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
            if (dgv == null || !dgv.IsHandleCreated || this.IsDisposed) return;

            if (dgv.InvokeRequired)
            {
                dgv.BeginInvoke(new Action(() => UpdateGridDataSource(dgv, dt)));
                return;
            }

            try
            {
                int scrollIdx = dgv.FirstDisplayedScrollingRowIndex;
                int selectedIdx = dgv.CurrentRow?.Index ?? -1;

                dgv.DataSource = dt;

                if (scrollIdx >= 0 && scrollIdx < dgv.RowCount) dgv.FirstDisplayedScrollingRowIndex = scrollIdx;
                if (selectedIdx >= 0 && selectedIdx < dgv.RowCount) dgv.Rows[selectedIdx].Selected = true;

                ApplyCustomFormatting(dgv);
            }
            catch { }
        }

        private void ApplyCustomFormatting(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                // 번호 매기기 (첫 번째 열)
                if (dgv.Columns.Contains("No")) row.Cells["No"].Value = row.Index + 1;

                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.OwningColumn == null || cell.Value == null) continue;
                    string colName = cell.OwningColumn.Name;
                    string valStr = cell.Value.ToString();

                    // 1. 수익률 / 등락률 색상 입히기
                    if (colName == "Pnl_Rate" || colName == "Return_1m" || colName == "Order_Yield")
                    {
                        if (double.TryParse(valStr.Replace("%", "").Replace(",", ""), out double val))
                        {
                            if (val > 0) cell.Style.ForeColor = Color.Red;
                            else if (val < 0) cell.Style.ForeColor = Color.DeepSkyBlue;
                            else cell.Style.ForeColor = Color.WhiteSmoke;

                            // % 기호 붙여주기
                            cell.Value = $"{val:F2}%";
                        }
                    }

                    // 2. 주문 종류 및 상태를 한글로 완벽 번역 (BUY -> 매수)
                    else if (colName == "Order_Type" || colName == "Status" || colName == "Signal")
                    {
                        string upperVal = valStr.ToUpper();
                        if (upperVal.Contains("SELL_PROFIT")) { cell.Value = "익절"; cell.Style.ForeColor = Color.DeepSkyBlue; }
                        else if (upperVal.Contains("SELL_LOSS")) { cell.Value = "손절"; cell.Style.ForeColor = Color.DeepSkyBlue; }
                        else if (upperVal.Contains("SELL") || upperVal.Contains("매도")) { cell.Value = "매도"; cell.Style.ForeColor = Color.DeepSkyBlue; }
                        else if (upperVal.Contains("BUY") || upperVal.Contains("매수")) { cell.Value = "매수"; cell.Style.ForeColor = Color.Red; }
                        else if (upperVal.Contains("WAIT")) { cell.Value = "대기"; cell.Style.ForeColor = Color.Yellow; }
                    }

                    // 3. 숫자가 들어간 칸은 천단위 콤마(,)와 함께 소수점 싹 제거 (한국 주식 맞춤 정수)
                    else if (colName.Contains("Price") || colName.Contains("Quantity") || colName.Contains("Amount") || colName.Contains("Pnl") || colName.Contains("Cash") || colName == "Ma_5" || colName == "Ma_20" || colName == "Volume")
                    {
                        if (double.TryParse(valStr.Replace(",", ""), out double numVal))
                        {
                            cell.Value = numVal.ToString("N0"); // 소수점 버리고 정수로 변환
                        }
                    }
                }
            }
        }

        public void UIOrganize()
        {
            if (dgvChart1 == null || dgvChart2 == null || dgvChart3 == null || dgvChart4 == null) return;
            dgvChartArray = new DataGridView[] { dgvChart1, dgvChart2, dgvChart3, dgvChart4 };

            for (int i = 0; i < dgvChartArray.Length; i++)
            {
                dgvChartArray[i].SelectionChanged -= DgvChart_SelectionChanged;
                dgvChartArray[i].SelectionChanged += DgvChart_SelectionChanged;
                SetGridStyle(dgvChartArray[i], i);
                dgvChartArray[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }

        private void SetGridStyle(DataGridView dgv, int ChartIndex)
        {
            // 🔥 [핵심] AutoGenerateColumns를 false로 해야 표가 중복해서 생기지 않습니다!
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

            // 컬럼 매핑: DataPropertyName 을 DB_Manager.cs 의 Alias 와 똑같이 맞춰줍니다.
            if (dgv.Columns.Count == 0 && ChartIndex == 0) // 마켓
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Last_Price", "현재가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Open_Price", "시가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "High_Price", "고가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Low_Price", "저가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Return_1m", "1분등락률", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Trade_Amount", "거래대금", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Vol_Energy", "거래량에너지", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Disparity", "이격도", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Volume", "거래량", 60, DataGridViewContentAlignment.MiddleCenter);
            }
            else if (dgv.Columns.Count == 0 && ChartIndex == 1) // 계좌
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);
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
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Type", "주문종류", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Price", "주문가격", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Quantity", "주문수량", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Filled_Quqntity", "체결수량", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Time", "주문시간", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Status", "주문상태", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Order_Yield", "수익률", 60, DataGridViewContentAlignment.MiddleCenter);
            }
            else if (dgv.Columns.Count == 0 && ChartIndex == 3) // 전략
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);
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
            // 🔥 마법의 매핑: DB에서 가져온 'Symbol'이 UI의 '종목코드' 헤더 칸에 자동으로 들어갑니다.
            dgv.Columns[idx].DataPropertyName = dataPropertyName;
            dgv.Columns[idx].Width = width;
            dgv.Columns[idx].DefaultCellStyle.Alignment = align;
        }

        private void DgvChart_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = sender as DataGridView;
                if (dgv == null || dgv.CurrentRow == null) return;

                string symbol = dgv.CurrentRow.Cells["Symbol"].Value?.ToString();

                if (string.IsNullOrWhiteSpace(symbol) || symbol == "-") return;

                var chartForm = Application.OpenForms.OfType<FormGraphic>().FirstOrDefault();
                chartForm?.ShowStock(symbol);
            }
            catch { }
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