using System;
using System.Data;
using System.Windows.Forms;

namespace inventory_management_system_with_POS
{
    // ==========================================================
    // DAMAGE REPORT  ->  tbl_stock_movement (OUT: Damaged / Adjustment)
    //
    //   Filters : date range, reason, search (product id / name / remarks)
    //   Summary : records, units, total loss at cost
    //   Report Damage : pick a product, then the normal stock dialog
    //                   opens already set to "Stock out - Damaged".
    // ==========================================================
    public partial class FrmDamagereport : frmReportBase
    {
        private readonly ComboBox _cmbReason;

        public FrmDamagereport() : base("DAMAGE REPORT", "damage_report", true)
        {
            InitializeComponent();

            _cmbReason = new ComboBox();
            _cmbReason.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbReason.Width = 130;
            _cmbReason.Items.AddRange(new object[] { "All", "Damaged", "Adjustment" });
            _cmbReason.SelectedIndex = 0;
            AddFilter("Reason:", _cmbReason);

            AddButton("Report Damage", btnReportDamage_Click);
        }

        protected override void LoadData()
        {
            string reason = _cmbReason.SelectedIndex <= 0 ? "" : _cmbReason.Text;

            DataTable dt = InventoryData.GetStockOutReport(
                DtpFrom.Value, DtpTo.Value, reason, TxtSearch.Text);

            Grid.DataSource = dt;

            HideColumn(Grid, "movement_id");
            Format(Grid, "created_at", "Date", "yyyy-MM-dd hh:mm tt", false);
            Format(Grid, "product_id", "Product ID", null, false);
            Format(Grid, "product_name", "Product", null, false);
            Format(Grid, "category_name", "Category", null, false);
            Format(Grid, "reason", "Reason", null, false);
            Format(Grid, "quantity", "Qty", "N0", true);
            Format(Grid, "unit_cost", "Unit cost", "N2", true);
            Format(Grid, "loss", "Loss value", "N2", true);
            Format(Grid, "reported_by", "Reported by", null, false);
            Format(Grid, "remarks", "Remarks", null, false);

            int units = 0;
            decimal loss = 0m;

            foreach (DataRow r in dt.Rows)
            {
                units += Int(r["quantity"]);
                loss += Dec(r["loss"]);
            }

            SummaryLabel.Text = "Records: " + dt.Rows.Count
                + "     Units: " + units.ToString("N0")
                + "     Total loss (at cost): " + Peso + loss.ToString("N2");
        }

        private void btnReportDamage_Click(object sender, EventArgs e)
        {
            string id;
            string name;
            int stock;
            decimal cost;

            using (frmProductPickDialog pick = new frmProductPickDialog())
            {
                if (pick.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                id = pick.ProductId;
                name = pick.ProductName;
                stock = pick.Stock;
                cost = pick.Cost;
            }

            if (stock <= 0)
            {
                MessageBox.Show("\"" + name + "\" has no stock to report as damaged.",
                    "Report Damage", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 1 = "Stock out - Damaged"
            using (frmStockMovementDialog dlg = new frmStockMovementDialog(id, name, stock, cost, 1))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    Reload();
                }
            }
        }
    }
}
