using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace inventory_management_system_with_POS
{
    // ==========================================================
    // SALES REPORT  ->  tbl_sale / tbl_sale_item
    //
    //   Filters : date range, status (All / Completed / Cancelled),
    //             search (sale no., cashier, payment method)
    //   Summary : completed sales + total, discounts, cancelled, average
    //   Lower grid : the items of the selected sale
    //   View receipt : reprint of the selected sale
    //   Cancel sale  : Manager / Admin only. Marks the sale 'Cancelled' and
    //                  puts every item back into stock (same batch/expiry).
    // ==========================================================
    public partial class frmManageReport : frmReportBase
    {
        private readonly ComboBox _cmbStatus;
        private readonly DataGridView _items;
        private readonly Label _lblItems;
        private Font _strikeFont;

        public frmManageReport() : base("SALES REPORT", "sales_report", true)
        {
            InitializeComponent();

            _cmbStatus = new ComboBox();
            _cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbStatus.Width = 120;
            _cmbStatus.Items.AddRange(new object[] { "All", "Completed", "Cancelled" });
            _cmbStatus.SelectedIndex = 0;
            AddFilter("Status:", _cmbStatus);

            // ---- items of the selected sale (under the main grid) ----
            _items = CreateGrid();
            _items.Dock = DockStyle.Bottom;
            _items.Height = 150;

            _lblItems = new Label();
            _lblItems.Dock = DockStyle.Bottom;
            _lblItems.Height = 26;
            _lblItems.Padding = new Padding(22, 0, 0, 0);
            _lblItems.TextAlign = ContentAlignment.MiddleLeft;
            _lblItems.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblItems.Text = "Items of the selected sale";

            // Bottom docks run from the end of the list: the grid first, the label above it.
            Controls.Add(_lblItems);
            Controls.Add(_items);

            // The button bar must stay the outermost bar at the bottom.
            Controls.SetChildIndex(ButtonBar, Controls.Count - 1);

            AddButton("View receipt", btnReceipt_Click);
            AddButton("Cancel sale", btnCancelSale_Click);

            Grid.SelectionChanged += Grid_SelectionChanged;
            Grid.DataBindingComplete += Grid_DataBindingComplete;
        }

        protected override void LoadData()
        {
            string status = _cmbStatus.SelectedIndex <= 0 ? "" : _cmbStatus.Text;

            DataTable dt = InventoryData.GetSalesReport(DtpFrom.Value, DtpTo.Value, status, TxtSearch.Text);

            Grid.DataSource = dt;

            Format(Grid, "sale_id", "Sale No.", null, false);
            Format(Grid, "created_at", "Date", "yyyy-MM-dd hh:mm tt", false);
            Format(Grid, "cashier", "Cashier", null, false);
            Format(Grid, "items", "Items", "N0", true);
            Format(Grid, "subtotal", "Subtotal", "N2", true);
            Format(Grid, "discount_amount", "Discount", "N2", true);
            Format(Grid, "tax_amount", "Tax", "N2", true);
            Format(Grid, "total_amount", "Total", "N2", true);
            Format(Grid, "amount_paid", "Paid", "N2", true);
            Format(Grid, "change_due", "Change", "N2", true);
            Format(Grid, "payment_method", "Payment", null, false);
            Format(Grid, "sale_status", "Status", null, false);

            int completed = 0;
            int cancelled = 0;
            decimal completedTotal = 0m;
            decimal cancelledTotal = 0m;
            decimal discounts = 0m;

            foreach (DataRow r in dt.Rows)
            {
                if (string.Equals(Convert.ToString(r["sale_status"]), "Completed", StringComparison.OrdinalIgnoreCase))
                {
                    completed++;
                    completedTotal += Dec(r["total_amount"]);
                    discounts += Dec(r["discount_amount"]);
                }
                else
                {
                    cancelled++;
                    cancelledTotal += Dec(r["total_amount"]);
                }
            }

            decimal average = completed > 0 ? completedTotal / completed : 0m;

            SummaryLabel.Text = "Completed: " + completed + "  (" + Peso + completedTotal.ToString("N2") + ")"
                + "     Average sale: " + Peso + average.ToString("N2")
                + "     Discounts given: " + Peso + discounts.ToString("N2")
                + "     Cancelled: " + cancelled + "  (" + Peso + cancelledTotal.ToString("N2") + ")";

            ShowItems();
        }

        private void Grid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in Grid.Rows)
            {
                DataRowView view = row.DataBoundItem as DataRowView;

                if (view != null && !string.Equals(Convert.ToString(view["sale_status"]), "Completed",
                        StringComparison.OrdinalIgnoreCase))
                {
                    row.DefaultCellStyle.ForeColor = Color.Gray;
                    if (_strikeFont == null)
                    {
                        _strikeFont = new Font(Grid.Font, FontStyle.Strikeout);
                    }

                    row.DefaultCellStyle.Font = _strikeFont;
                }
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            ShowItems();
        }

        private void ShowItems()
        {
            DataRow sale = SelectedRow();

            if (sale == null)
            {
                _items.DataSource = null;
                _lblItems.Text = "Items of the selected sale";
                return;
            }

            try
            {
                string id = Convert.ToString(sale["sale_id"]);

                _items.DataSource = InventoryData.GetSaleItems(id);
                _lblItems.Text = "Items of " + id;

                Format(_items, "product_id", "Product ID", null, false);
                Format(_items, "product_name", "Product", null, false);
                Format(_items, "quantity", "Qty", "N0", true);
                Format(_items, "unit_price", "Unit price", "N2", true);
                Format(_items, "line_discount", "Discount", "N2", true);
                Format(_items, "line_total", "Line total", "N2", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------
        // Receipt reprint
        // ------------------------------------------------------------
        private void btnReceipt_Click(object sender, EventArgs e)
        {
            DataRow sale = SelectedRow();

            if (sale == null)
            {
                MessageBox.Show("Select a sale in the list first.", "Receipt",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string text = BuildReceipt(sale, InventoryData.GetSaleItems(Convert.ToString(sale["sale_id"])));

                using (frmReceiptDialog dlg = new frmReceiptDialog(text))
                {
                    dlg.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Receipt", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string BuildReceipt(DataRow sale, DataTable items)
        {
            const int width = 38;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Center("Mikmik Sales", width));
            sb.AppendLine(Center("*** REPRINT ***", width));
            sb.AppendLine(new string('-', width));
            sb.AppendLine("Receipt : " + Convert.ToString(sale["sale_id"]));
            sb.AppendLine("Date    : " + Convert.ToDateTime(sale["created_at"]).ToString("yyyy-MM-dd hh:mm tt"));
            sb.AppendLine("Cashier : " + (sale["cashier"] == DBNull.Value ? "-" : Convert.ToString(sale["cashier"])));
            sb.AppendLine("Status  : " + Convert.ToString(sale["sale_status"]));
            sb.AppendLine(new string('-', width));

            foreach (DataRow line in items.Rows)
            {
                string name = Convert.ToString(line["product_name"]);

                if (name.Length > width)
                {
                    name = name.Substring(0, width);
                }

                sb.AppendLine(name);
                sb.AppendLine(Row(
                    "  " + Int(line["quantity"]) + " x " + Dec(line["unit_price"]).ToString("N2"),
                    Dec(line["line_total"]).ToString("N2"), width));

                if (Dec(line["line_discount"]) > 0)
                {
                    sb.AppendLine(Row("  discount", "-" + Dec(line["line_discount"]).ToString("N2"), width));
                }
            }

            sb.AppendLine(new string('-', width));
            sb.AppendLine(Row("Subtotal", Dec(sale["subtotal"]).ToString("N2"), width));

            if (Dec(sale["discount_amount"]) > 0)
            {
                sb.AppendLine(Row("Discount", "-" + Dec(sale["discount_amount"]).ToString("N2"), width));
            }

            if (Dec(sale["tax_amount"]) > 0)
            {
                sb.AppendLine(Row("Tax", Dec(sale["tax_amount"]).ToString("N2"), width));
            }

            sb.AppendLine(Row("TOTAL", Dec(sale["total_amount"]).ToString("N2"), width));
            sb.AppendLine(Row(Convert.ToString(sale["payment_method"]), Dec(sale["amount_paid"]).ToString("N2"), width));
            sb.AppendLine(Row("Change", Dec(sale["change_due"]).ToString("N2"), width));
            sb.AppendLine(new string('-', width));
            sb.AppendLine(Center("Thank you!", width));

            return sb.ToString();
        }

        private static string Center(string text, int width)
        {
            if (text.Length >= width)
            {
                return text;
            }

            return new string(' ', (width - text.Length) / 2) + text;
        }

        private static string Row(string left, string right, int width)
        {
            int gap = width - left.Length - right.Length;

            if (gap < 1)
            {
                gap = 1;
            }

            return left + new string(' ', gap) + right;
        }

        // ------------------------------------------------------------
        // Cancel sale
        // ------------------------------------------------------------
        private void btnCancelSale_Click(object sender, EventArgs e)
        {
            if (!Session.Can(Session.Manager))   // Admin always passes
            {
                MessageBox.Show("Only a Manager or Admin can cancel a sale.", "Cancel sale",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataRow sale = SelectedRow();

            if (sale == null)
            {
                MessageBox.Show("Select a sale in the list first.", "Cancel sale",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string id = Convert.ToString(sale["sale_id"]);

            if (!string.Equals(Convert.ToString(sale["sale_status"]), "Completed", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Sale " + id + " is already " + Convert.ToString(sale["sale_status"]) + ".",
                    "Cancel sale", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(
                    "Cancel sale " + id + " (" + Peso + Dec(sale["total_amount"]).ToString("N2") + ")?\n\n"
                    + "The items go back into stock. This cannot be undone.",
                    "Cancel sale", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                InventoryData.CancelSale(id, "Cancelled by " + Session.AccountName);
                Reload();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Cancel sale", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
