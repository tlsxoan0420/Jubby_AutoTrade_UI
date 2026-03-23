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
using Jubby_AutoTrade_UI.COMMON;
using Jubby_AutoTrade_UI.SEQUENCE;
using static Jubby_AutoTrade_UI.COMMON.Flag;

namespace Jubby_AutoTrade_UI.GUI
{
    public partial class FormDataChart : Form
    {
        #region ## FormDataChart Define ##
        private DataGridView[] dgvChartArray;
        private Dictionary<string, DataGridViewRow>[] ChartrowMaps;
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
                dgvChart1, // 0번: 시세 정보
                dgvChart2, // 1번: 내 잔고 정보
                dgvChart3, // 2번: 주문 내역 데이터
                dgvChart4, // 3번: 전략 분석 정보
            };

            ChartrowMaps = new Dictionary<string, DataGridViewRow>[4];

            for (int i = 0; i < dgvChartArray.Length; i++)
            {
                ChartrowMaps[i] = new Dictionary<string, DataGridViewRow>();
                dgvChartArray[i].SelectionChanged -= DgvChart_SelectionChanged;
                dgvChartArray[i].SelectionChanged += DgvChart_SelectionChanged;
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
        public void UIUpdate() { }
        #endregion ## UI Update ##

        #region ## Chart Event ##

        #region ## Set Grid Style ##
        private void SetGridStyle(DataGridView dgv, int Chart)
        {
            dgv.DoubleBuffered(true);
            dgv.AllowUserToAddRows = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly = true;
            dgv.BorderStyle = BorderStyle.None;

            dgv.BackgroundColor = Color.FromArgb(5, 5, 15);
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(5, 5, 15);
            dgv.DefaultCellStyle.ForeColor = Color.WhiteSmoke;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 60);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.GridColor = Color.FromArgb(70, 70, 90);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 20, 35);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.LightGray;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("HY헤드라인M", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 100;

            if (dgv.Columns.Count == 0 && Chart == 0)
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Last_Price", "현재가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Open_Price", "시가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "High_Price", "고가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Low_Price", "저가", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Return_1m", "1분 등락률", 80, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Trade_Amount", "거래대금", 80, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Vol_Energy", "볼륨에너지", 70, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Disparity", "이격도", 70, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Volume", "거래량", 60, DataGridViewContentAlignment.MiddleCenter);
            }
            else if (dgv.Columns.Count == 0 && Chart == 1)
            {
                AddColumn(dgv, "No", "번호", 40, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Symbol", "종목코드", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Name", "종목명", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Quantity", "보유수량", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Avg_Price", "평균매입가", 80, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Current_Price", "현재가", 80, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Pnl", "평가손익", 60, DataGridViewContentAlignment.MiddleCenter);
                AddColumn(dgv, "Available_Cash", "주문가능 금액", 80, DataGridViewContentAlignment.MiddleCenter);
            }
            else if (dgv.Columns.Count == 0 && Chart == 2)
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
            }
            else if (dgv.Columns.Count == 0 && Chart == 3)
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
            if (targetIndex < 0 || targetIndex >= dgvChartArray.Length) return;

            DataGridView targetGrid = dgvChartArray[targetIndex];
            Dictionary<string, DataGridViewRow> targetMap = ChartrowMaps[targetIndex];

            if (targetGrid.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateData(targetIndex, stock, target)));
                return;
            }

            if (targetIndex == 2)
            {
                var orders = stock.GetOrderListSafe();
                if (orders != null && orders.Count > 0)
                {
                    foreach (var order in orders)
                    {
                        bool isDuplicate = false;
                        foreach (DataGridViewRow r in targetGrid.Rows)
                        {
                            if (r.Cells["Symbol"].Value?.ToString() == stock.Symbol &&
                                r.Cells["Order_Time"].Value?.ToString() == order.Order_Time)
                            {
                                isDuplicate = true; break;
                            }
                        }

                        if (!isDuplicate)
                        {
                            int rowIdx = targetGrid.Rows.Add();
                            DataGridViewRow newRow = targetGrid.Rows[rowIdx];
                            newRow.Tag = stock;
                            newRow.Cells["No"].Value = rowIdx + 1;
                            newRow.Cells["Symbol"].Value = stock.Symbol;
                            newRow.Cells["Name"].Value = stock.Name;
                            newRow.Cells["Order_Type"].Value = order.Order_Type;
                            newRow.Cells["Order_Price"].Value = order.Order_Price.ToString("N0");
                            newRow.Cells["Order_Quantity"].Value = order.Order_Quantity.ToString("N0");
                            newRow.Cells["Filled_Quqntity"].Value = order.Filled_Quqntity.ToString("N0");
                            newRow.Cells["Order_Time"].Value = order.Order_Time;
                            newRow.Cells["Status"].Value = order.Status;

                            var chartForm = Application.OpenForms.OfType<FormGraphic>().FirstOrDefault();

                            if (chartForm != null && order.Order_Price > 0)
                            {
                                string oType = order.Order_Type;
                                if (oType == "BUY" || oType.Contains("SELL"))
                                {
                                    chartForm.AddOrderMarker(stock.Symbol, oType, order.Order_Price, order.Order_Time);

                                    if (oType == "BUY") System.Media.SystemSounds.Beep.Play();
                                    else if (oType == "SELL_PROFIT") System.Media.SystemSounds.Asterisk.Play();
                                    else if (oType == "SELL_LOSS") System.Media.SystemSounds.Exclamation.Play();
                                }
                            }
                        }
                    }
                }

                if (targetGrid.Rows.Count > 500)
                {
                    targetGrid.Rows.RemoveAt(0);
                    ReindexRows(targetGrid);
                }

                if (targetGrid.Rows.Count > 0)
                    targetGrid.FirstDisplayedScrollingRowIndex = targetGrid.Rows.Count - 1;

                targetGrid.Refresh();
                return;
            }

            if (targetMap.ContainsKey(stock.Symbol))
            {
                DataGridViewRow row = targetMap[stock.Symbol];
                SetRowValues(row, stock, target);
                targetGrid.Refresh();
            }
            else
            {
                int rowIndex = targetGrid.Rows.Add();
                DataGridViewRow row = targetGrid.Rows[rowIndex];
                row.Tag = stock;
                targetMap.Add(stock.Symbol, row);

                row.Cells["No"].Value = rowIndex + 1;
                row.Cells["Symbol"].Value = stock.Symbol;
                row.Cells["Name"].Value = stock.Name;

                SetRowValues(row, stock, target);
                targetGrid.Refresh();
            }
        }

        private void SetRowValues(DataGridViewRow row, Flag.JubbyStockInfo stock, Flag.UpdateTarget target)
        {
            if (target == Flag.UpdateTarget.All || target == Flag.UpdateTarget.Market)
            {
                row.Cells["Last_Price"].Value = stock.Market.Last_Price.ToString("N0");
                row.Cells["Open_Price"].Value = stock.Market.Open_Price.ToString("N0");
                row.Cells["High_Price"].Value = stock.Market.High_Price.ToString("N0");
                row.Cells["Low_Price"].Value = stock.Market.Low_Price.ToString("N0");
                row.Cells["Return_1m"].Value = stock.Market.Return_1m.ToString("N2");
                row.Cells["Trade_Amount"].Value = stock.Market.Trade_Amount.ToString("N0");
                row.Cells["Vol_Energy"].Value = stock.Market.Vol_Energy.ToString("N2");
                row.Cells["Disparity"].Value = stock.Market.Disparity.ToString("N2");
                row.Cells["Volume"].Value = stock.Market.Volume.ToString("N0");

                if (stock.Market.Return_1m > 0) row.Cells["Return_1m"].Style.ForeColor = Color.Lime;
                else if (stock.Market.Return_1m < 0) row.Cells["Return_1m"].Style.ForeColor = Color.DeepSkyBlue;
                else row.Cells["Return_1m"].Style.ForeColor = Color.WhiteSmoke;

                if (stock.Market.Vol_Energy >= 2.5m) row.Cells["Vol_Energy"].Style.ForeColor = Color.Orange;
                else row.Cells["Vol_Energy"].Style.ForeColor = Color.WhiteSmoke;
            }

            if (target == Flag.UpdateTarget.All || target == Flag.UpdateTarget.Account)
            {
                row.Cells["Quantity"].Value = stock.MyAccount.Quantity.ToString("N0");
                row.Cells["Avg_Price"].Value = stock.MyAccount.Avg_Price.ToString("N0");
                row.Cells["Current_Price"].Value = stock.MyAccount.Current_Price.ToString("N0");
                row.Cells["Pnl"].Value = stock.MyAccount.Pnl.ToString("N2");
                row.Cells["Available_Cash"].Value = stock.MyAccount.Available_Cash.ToString("N0");

                if (stock.MyAccount.Pnl > 0) row.Cells["Pnl"].Style.ForeColor = Color.Lime;
                else if (stock.MyAccount.Pnl < 0) row.Cells["Pnl"].Style.ForeColor = Color.DeepSkyBlue;
                else row.Cells["Pnl"].Style.ForeColor = Color.WhiteSmoke;
            }

            if (target == Flag.UpdateTarget.All || target == Flag.UpdateTarget.Strategy)
            {
                if (stock.Strategy != null)
                {
                    row.Cells["Ma_5"].Value = stock.Strategy.Ma_5.ToString("N2");
                    row.Cells["Ma_20"].Value = stock.Strategy.Ma_20.ToString("N2");
                    row.Cells["RSI"].Value = stock.Strategy.RSI.ToString("N2");
                    row.Cells["MACD"].Value = stock.Strategy.MACD.ToString("N2");
                    row.Cells["Signal"].Value = stock.Strategy.Signal;

                    if (stock.Strategy.Signal == "BUY") row.Cells["Signal"].Style.ForeColor = Color.Lime;
                    else if (stock.Strategy.Signal == "SELL") row.Cells["Signal"].Style.ForeColor = Color.DeepSkyBlue;
                    else row.Cells["Signal"].Style.ForeColor = Color.WhiteSmoke;
                }
            }
        }
        #endregion ## Update Chart Data ##

        #region ## Remove Chart Data ##
        private void RemoveData(int targetIndex, string symbol, Flag.UpdateTarget target)
        {
            if (targetIndex < 0 || targetIndex >= dgvChartArray.Length) return;

            DataGridView targetGrid = dgvChartArray[targetIndex];
            Dictionary<string, DataGridViewRow> targetMap = ChartrowMaps[targetIndex];

            if (targetGrid.InvokeRequired)
            {
                this.Invoke(new Action(() => RemoveData(targetIndex, symbol, target)));
                return;
            }

            if (targetMap.ContainsKey(symbol))
            {
                DataGridViewRow row = targetMap[symbol];
                targetGrid.Rows.Remove(row);
                targetMap.Remove(symbol);

                if (target == Flag.UpdateTarget.Order)
                {
                    if (ChartrowMaps[targetIndex].ContainsKey(symbol))
                    {
                        row = ChartrowMaps[targetIndex][symbol];
                        if (row.Tag is Flag.JubbyStockInfo stock)
                        {
                            stock.RemoveOrder(symbol);
                        }
                    }
                }
                ReindexRows(targetGrid);
            }
        }

        private void ReindexRows(DataGridView dgv)
        {
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                dgv.Rows[i].Cells["No"].Value = i + 1;
            }
        }
        #endregion ## Remove Chart Data ##

        #region ## Apply Stock Update ##

        // 💡 [에러 해결] 에러 메시지에 뜨던 누락된 함수 완벽 복구
        internal void ApplyStockUpdate(JubbyStockInfo stock, UpdateTarget target)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ApplyStockUpdate(stock, target)));
                return;
            }

            if (target == UpdateTarget.Market || target == UpdateTarget.All)
                UpdateData(0, stock, UpdateTarget.Market);

            if (target == UpdateTarget.Account || target == UpdateTarget.All)
                UpdateData(1, stock, UpdateTarget.Account);

            if (target == UpdateTarget.Order || target == UpdateTarget.All)
                UpdateData(2, stock, UpdateTarget.Order);

            if (target == UpdateTarget.Strategy || target == UpdateTarget.All)
                UpdateData(3, stock, UpdateTarget.Strategy);
        }

        // 💡 [에러 해결] 에러 메시지에 뜨던 누락된 함수 완벽 복구
        internal void SafeApplyStockUpdate(JubbyStockInfo stock, UpdateTarget target)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => ApplyStockUpdate(stock, target)));
            }
            else
            {
                ApplyStockUpdate(stock, target);
            }
        }
        #endregion ## Apply Stock Update ##

        #endregion ## Chart Event ##

        #region ## UI Event ##
        private void DgvChart_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = sender as DataGridView;
                if (dgv == null || dgv.CurrentRow == null) return;

                if (!dgv.Columns.Contains("Symbol")) return;

                string symbol = Convert.ToString(dgv.CurrentRow.Cells["Symbol"].Value);
                if (string.IsNullOrWhiteSpace(symbol) || symbol == "-") return;

                var chartForm = Application.OpenForms.OfType<FormGraphic>().FirstOrDefault();
                chartForm?.ShowStock(symbol);
            }
            catch { }
        }
        #endregion ## UI Event ##

        #region ## Timer Event ##
        private void timer1_Tick(object sender, EventArgs e) { }
        #endregion ## Timer Event ##

        private void dgvChart1_DoubleClick(object sender, EventArgs e)
        {
            Random rnd = new Random();
            var stock = new JubbyStockInfo(rnd.NextDouble().ToString(), "TestData");
            stock.Market.Last_Price = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Open_Price = (decimal)rnd.NextDouble() * 1000;
            stock.Market.High_Price = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Low_Price = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Return_1m = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Trade_Amount = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Vol_Energy = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Disparity = (decimal)rnd.NextDouble() * 1000;
            stock.Market.Volume = (decimal)rnd.NextDouble() * 1000;
            UpdateData(0, stock, Flag.UpdateTarget.Market);
        }
    }

    #region ## DataGridView Extension Method ##
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
    #endregion ## DataGridView Extension Method ##
}