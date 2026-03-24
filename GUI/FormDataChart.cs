using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Jubby_AutoTrade_UI.COMMON;
using static Jubby_AutoTrade_UI.COMMON.Flag;

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormDataChart : Form
    {
        private DataGridView[] dgvChartArray;
        // Dictionary 맵 방식 대신 직접 검색 방식을 사용하여 안정성을 높입니다.

        public FormDataChart() { InitializeComponent(); UIOrganize(); }

        public void UIOrganize()
        {
            dgvChartArray = new DataGridView[] { dgvChart1, dgvChart2, dgvChart3, dgvChart4 };

            for (int i = 0; i < dgvChartArray.Length; i++)
            {
                dgvChartArray[i].SelectionChanged -= DgvChart_SelectionChanged;
                dgvChartArray[i].SelectionChanged += DgvChart_SelectionChanged;
                SetGridStyle(dgvChartArray[i], i);
                dgvChartArray[i].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }

        private void FormDataChart_Load(object sender, EventArgs e) { UIUpdate(); }
        public void UIUpdate() { }

        private void SetGridStyle(DataGridView dgv, int Chart)
        {
            dgv.DoubleBuffered(true); dgv.AllowUserToAddRows = false; dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgv.ReadOnly = true; dgv.BorderStyle = BorderStyle.None;
            dgv.BackgroundColor = Color.FromArgb(5, 5, 15); dgv.DefaultCellStyle.BackColor = Color.FromArgb(5, 5, 15);
            dgv.DefaultCellStyle.ForeColor = Color.WhiteSmoke; dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 60);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White; dgv.GridColor = Color.FromArgb(70, 70, 90);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single; dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 20, 35);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.LightGray;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("HY헤드라인M", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 100;

            // 껍데기 강제 리셋 (밀림 방지)
            dgv.Columns.Clear();

            if (Chart == 0) // 시세
            {
                AddColumn(dgv, "No", "번호", 40); AddColumn(dgv, "Symbol", "종목코드", 60); AddColumn(dgv, "Name", "종목명", 60);
                AddColumn(dgv, "Last_Price", "현재가", 60); AddColumn(dgv, "Open_Price", "시가", 60); AddColumn(dgv, "High_Price", "고가", 60);
                AddColumn(dgv, "Low_Price", "저가", 60); AddColumn(dgv, "Return_1m", "1분 등락률", 80); AddColumn(dgv, "Trade_Amount", "거래대금", 80);
                AddColumn(dgv, "Vol_Energy", "볼륨에너지", 70); AddColumn(dgv, "Disparity", "이격도", 70); AddColumn(dgv, "Volume", "거래량", 60);
            }
            else if (Chart == 1) // 잔고
            {
                AddColumn(dgv, "No", "번호", 40); AddColumn(dgv, "Symbol", "종목코드", 60); AddColumn(dgv, "Name", "종목명", 60);
                AddColumn(dgv, "Quantity", "보유수량", 60); AddColumn(dgv, "Avg_Price", "평균매입가", 80); AddColumn(dgv, "Current_Price", "현재가", 80);
                AddColumn(dgv, "Pnl_Amt", "평가손익금(원)", 100); AddColumn(dgv, "Pnl_Rate", "수익률(%)", 80); AddColumn(dgv, "Available_Cash", "주문가능금액", 100);
            }
            else if (Chart == 2) // 주문
            {
                AddColumn(dgv, "No", "번호", 40); AddColumn(dgv, "Symbol", "종목코드", 60); AddColumn(dgv, "Name", "종목명", 60);
                AddColumn(dgv, "Order_Type", "주문종류", 60); AddColumn(dgv, "Order_Price", "주문가격", 60); AddColumn(dgv, "Order_Quantity", "주문수량", 60);
                AddColumn(dgv, "Filled_Quqntity", "체결수량", 60); AddColumn(dgv, "Order_Time", "주문시간", 80); AddColumn(dgv, "Status", "주문상태", 60);
                AddColumn(dgv, "Order_Yield", "수익률", 80);
            }
            else if (Chart == 3) // 전략
            {
                AddColumn(dgv, "No", "번호", 40); AddColumn(dgv, "Symbol", "종목코드", 60); AddColumn(dgv, "Name", "종목명", 60);
                AddColumn(dgv, "Ma_5", "단기 이동평균", 80); AddColumn(dgv, "Ma_20", "장기 이동평균", 80); AddColumn(dgv, "RSI", "RSI 지표", 60);
                AddColumn(dgv, "MACD", "MACD 지표", 60); AddColumn(dgv, "Signal", "전략 신호", 60);
            }
        }

        private void AddColumn(DataGridView dgv, string name, string header, int width)
        {
            int idx = dgv.Columns.Add(name, header);
            dgv.Columns[idx].Width = width;
            dgv.Columns[idx].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void UpdateData(int targetIndex, Flag.JubbyStockInfo stock, Flag.UpdateTarget target)
        {
            if (targetIndex < 0 || targetIndex >= dgvChartArray.Length) return;
            DataGridView targetGrid = dgvChartArray[targetIndex];

            if (targetGrid.InvokeRequired) { this.Invoke(new Action(() => UpdateData(targetIndex, stock, target))); return; }

            try
            {
                // 1. 주문 내역(index 2)은 데이터가 계속 쌓여야 하므로 로직 분리
                if (targetIndex == 2)
                {
                    var orders = stock.GetOrderListSafe();
                    if (orders == null) return;

                    foreach (var order in orders)
                    {
                        bool isDuplicate = false;
                        foreach (DataGridViewRow r in targetGrid.Rows)
                        {
                            if (r.Cells["Symbol"].Value?.ToString() == stock.Symbol &&
                                r.Cells["Order_Time"].Value?.ToString() == order.Order_Time)
                            { isDuplicate = true; break; }
                        }

                        if (!isDuplicate)
                        {
                            int rowIdx = targetGrid.Rows.Add();
                            DataGridViewRow newRow = targetGrid.Rows[rowIdx];
                            newRow.Cells["No"].Value = rowIdx + 1;
                            newRow.Cells["Symbol"].Value = stock.Symbol;
                            newRow.Cells["Name"].Value = stock.Name;
                            newRow.Cells["Order_Type"].Value = order.Order_Type;
                            newRow.Cells["Order_Price"].Value = order.Order_Price.ToString("N0");
                            newRow.Cells["Order_Quantity"].Value = order.Order_Quantity.ToString("N0");
                            newRow.Cells["Filled_Quqntity"].Value = order.Filled_Quqntity.ToString("N0");
                            newRow.Cells["Order_Time"].Value = order.Order_Time;
                            newRow.Cells["Status"].Value = order.Status;

                            string yieldStr = string.IsNullOrEmpty(order.Order_Yield) ? "0.00%" : order.Order_Yield;
                            newRow.Cells["Order_Yield"].Value = yieldStr;
                            if (yieldStr.Contains("-")) newRow.Cells["Order_Yield"].Style.ForeColor = Color.DeepSkyBlue;
                            else if (yieldStr == "0.00%") newRow.Cells["Order_Yield"].Style.ForeColor = Color.WhiteSmoke;
                            else newRow.Cells["Order_Yield"].Style.ForeColor = Color.Lime;

                            // 차트 마커 표시
                            var chartForm = Application.OpenForms.OfType<FormGraphic>().FirstOrDefault();
                            if (chartForm != null && order.Order_Price > 0)
                            {
                                string oType = order.Order_Type ?? "";
                                if (oType == "BUY" || oType.Contains("SELL"))
                                    chartForm.AddOrderMarker(stock.Symbol, oType, order.Order_Price, order.Order_Time);
                            }
                        }
                    }
                    if (targetGrid.Rows.Count > 500) targetGrid.Rows.RemoveAt(0);
                    if (targetGrid.Rows.Count > 0) targetGrid.FirstDisplayedScrollingRowIndex = targetGrid.Rows.Count - 1;
                    return;
                }

                // 2. 나머지 표(시세, 잔고, 전략)는 종목당 1줄씩만 유지
                DataGridViewRow targetRow = null;
                foreach (DataGridViewRow r in targetGrid.Rows)
                {
                    if (r.Cells["Symbol"].Value?.ToString() == stock.Symbol)
                    {
                        targetRow = r;
                        break;
                    }
                }

                if (targetRow == null)
                {
                    int rowIndex = targetGrid.Rows.Add();
                    targetRow = targetGrid.Rows[rowIndex];
                    targetRow.Cells["No"].Value = rowIndex + 1;
                    targetRow.Cells["Symbol"].Value = stock.Symbol;
                    targetRow.Cells["Name"].Value = stock.Name;
                }

                SetRowValues(targetRow, stock, target);
                targetGrid.Refresh();
            }
            catch { }
        }

        // 🔥 어떤 에러가 발생해도 절대 튕기지 않고 안전하게 표에 값을 넣는 방탄 함수
        private void SetCellSafe(DataGridViewRow row, string colName, object value, Color? color = null)
        {
            if (row.DataGridView.Columns.Contains(colName))
            {
                row.Cells[colName].Value = value;
                if (color.HasValue) row.Cells[colName].Style.ForeColor = color.Value;
            }
        }

        private void SetRowValues(DataGridViewRow row, Flag.JubbyStockInfo stock, Flag.UpdateTarget target)
        {
            try
            {
                if (target == Flag.UpdateTarget.All || target == Flag.UpdateTarget.Market)
                {
                    SetCellSafe(row, "Last_Price", stock.Market.Last_Price.ToString("N0"));
                    SetCellSafe(row, "Open_Price", stock.Market.Open_Price.ToString("N0"));
                    SetCellSafe(row, "High_Price", stock.Market.High_Price.ToString("N0"));
                    SetCellSafe(row, "Low_Price", stock.Market.Low_Price.ToString("N0"));

                    Color retColor = stock.Market.Return_1m > 0 ? Color.Lime : (stock.Market.Return_1m < 0 ? Color.DeepSkyBlue : Color.WhiteSmoke);
                    SetCellSafe(row, "Return_1m", stock.Market.Return_1m.ToString("N2"), retColor);

                    SetCellSafe(row, "Trade_Amount", stock.Market.Trade_Amount.ToString("N0"));

                    Color volColor = stock.Market.Vol_Energy >= 2.5m ? Color.Orange : Color.WhiteSmoke;
                    SetCellSafe(row, "Vol_Energy", stock.Market.Vol_Energy.ToString("N2"), volColor);

                    SetCellSafe(row, "Disparity", stock.Market.Disparity.ToString("N2"));
                    SetCellSafe(row, "Volume", stock.Market.Volume.ToString("N0"));
                }

                if (target == Flag.UpdateTarget.All || target == Flag.UpdateTarget.Account)
                {
                    SetCellSafe(row, "Quantity", stock.MyAccount.Quantity.ToString("N0"));
                    SetCellSafe(row, "Avg_Price", stock.MyAccount.Avg_Price.ToString("N0"));
                    SetCellSafe(row, "Current_Price", stock.MyAccount.Current_Price.ToString("N0"));

                    Color pnlColor = stock.MyAccount.Pnl_Rate > 0 ? Color.Lime : (stock.MyAccount.Pnl_Rate < 0 ? Color.DeepSkyBlue : Color.WhiteSmoke);
                    SetCellSafe(row, "Pnl_Amt", stock.MyAccount.Pnl_Amt.ToString("N0"), pnlColor);
                    SetCellSafe(row, "Pnl_Rate", stock.MyAccount.Pnl_Rate.ToString("N2") + "%", pnlColor);

                    SetCellSafe(row, "Available_Cash", stock.MyAccount.Available_Cash.ToString("N0"));
                }

                if (target == Flag.UpdateTarget.All || target == Flag.UpdateTarget.Strategy)
                {
                    if (stock.Strategy != null)
                    {
                        SetCellSafe(row, "Ma_5", stock.Strategy.Ma_5.ToString("N2"));
                        SetCellSafe(row, "Ma_20", stock.Strategy.Ma_20.ToString("N2"));
                        SetCellSafe(row, "RSI", stock.Strategy.RSI.ToString("N2"));
                        SetCellSafe(row, "MACD", stock.Strategy.MACD.ToString("N2"));

                        Color sigColor = stock.Strategy.Signal == "BUY" ? Color.Lime : (stock.Strategy.Signal == "SELL" ? Color.DeepSkyBlue : Color.WhiteSmoke);
                        SetCellSafe(row, "Signal", stock.Strategy.Signal, sigColor);
                    }
                }
            }
            catch { } // 에러 무시
        }

        private void ReindexRows(DataGridView dgv) { for (int i = 0; i < dgv.Rows.Count; i++) dgv.Rows[i].Cells["No"].Value = i + 1; }

        internal void ApplyStockUpdate(JubbyStockInfo stock, UpdateTarget target)
        {
            if (this.IsDisposed) return;
            if (this.InvokeRequired) { this.BeginInvoke(new Action(() => ApplyStockUpdate(stock, target))); return; }

            if (target == UpdateTarget.Market || target == UpdateTarget.All) UpdateData(0, stock, UpdateTarget.Market);
            if (target == UpdateTarget.Account || target == UpdateTarget.All) UpdateData(1, stock, UpdateTarget.Account);
            if (target == UpdateTarget.Order || target == UpdateTarget.All) UpdateData(2, stock, UpdateTarget.Order);
            if (target == UpdateTarget.Strategy || target == UpdateTarget.All) UpdateData(3, stock, UpdateTarget.Strategy);
        }

        internal void SafeApplyStockUpdate(JubbyStockInfo stock, UpdateTarget target)
        {
            ApplyStockUpdate(stock, target);
        }

        private void DgvChart_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = sender as DataGridView;
                if (dgv == null || dgv.CurrentRow == null || !dgv.Columns.Contains("Symbol")) return;
                string symbol = Convert.ToString(dgv.CurrentRow.Cells["Symbol"].Value);
                if (string.IsNullOrWhiteSpace(symbol) || symbol == "-") return;
                var chartForm = Application.OpenForms.OfType<FormGraphic>().FirstOrDefault();
                chartForm?.ShowStock(symbol);
            }
            catch { }
        }

        private void timer1_Tick(object sender, EventArgs e) { }
        private void dgvChart1_DoubleClick(object sender, EventArgs e) { }
    }

    public static class ExtensionMethods
    {
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            System.Reflection.PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }
    }
}