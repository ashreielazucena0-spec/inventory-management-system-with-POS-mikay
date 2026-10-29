using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace inventory_management_system_with_POS
{
    // ==========================================================
    // EXPIRED ITEMS  ->  tbl_stock_movement (batches with an expiry date)
    //
    // A batch = product + batch_no + expiry_date. Its remaining quantity is
    // SUM(IN) - SUM(OUT) of that batch. The POS sells the batch that expires
    // first and never sells an expired batch, so expired stock stays on the
    // shelf (and in the stock count) until it is written off here.
    //
    // Stock that has no batch but belongs to a product with an expiry date
    // (set in Manage Product) is listed too.
    //
    //   Write off selected : stock OUT, reason "Expired", same batch/expiry
    //   Write off all expired : the same for every already-expired batch
    //
    // Row colours: red = expired, yellow = expires within 7 days.
    // ==========================================================
    public partial class frmExpireditem : frmReportBase
    {
        private readonly ComboBox _cmbShow;

        // 36500 days = "all with an expiry date".
        private readonly int[] _days = new int[] { -1, 7, 30, 60, 90, 36500 };

        public frmExpireditem() : base("EXPIRED ITEMS MANAGER", "expired_items", false)
        {
            InitializeComponent();

            _cmbShow = new ComboBox();
            _cmbShow.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbShow.Width = 190;
            _cmbShow.Items.AddRange(new object[]
            {
                "Already expired",
                "Expired + expiring in 7 days",
                "Expired + expiring in 30 days",
                "Expired + expiring in 60 days",
                "Expired + expiring in 90 days",
                "All with an expiry date"
            });
            _cmbShow.SelectedIndex = 5;
            AddFilter("Show:", _cmbShow);

            AddButton("Write off selected", btnWriteOffSelected_Click);
            AddButton("Write off all expired", btnWriteOffAll_Click);

            Grid.DataBindingComplete += Grid_DataBindingComplete;
        }

        // Harmless: only needed if the designer ever adds the Load line back.
        private void frmExpireditem_Load(object sender, EventArgs e)
        {
        }

        protected override void LoadData()
        {
            DataTable dt = InventoryData.GetExpiringBatches(_days[_cmbShow.SelectedIndex], TxtSearch.Text);

            Grid.DataSource = dt;

            Format(Grid, "product_id", "Product ID", null, false);
            Format(Grid, "product_name", "Product", null, false);
            Format(Grid, "category_name", "Category", null, false);
            Format(Grid, "batch_no", "Batch", null, false);
            Format(Grid, "expiry_date", "Expiry date", "yyyy-MM-dd", false);
            Format(Grid, "days_left", "Days left", "N0", true);
            Format(Grid, "remaining", "Qty", "N0", true);
            Format(Grid, "stock_value", "Value (cost)", "N2", true);

            int expiredBatches = 0;
            int expiredUnits = 0;
            decimal expiredValue = 0m;
            int soonUnits = 0;

            foreach (DataRow r in dt.Rows)
            {
                if (Int(r["days_left"]) < 0)
                {
                    expiredBatches++;
                    expiredUnits += Int(r["remaining"]);
                    expiredValue += Dec(r["stock_value"]);
                }
                else
                {
                    soonUnits += Int(r["remaining"]);
                }
            }

            SummaryLabel.Text = "Expired batches: " + expiredBatches
                + "     Expired units: " + expiredUnits.ToString("N0")
                + "     Expired value: " + Peso + expiredValue.ToString("N2")
                + "     Expiring soon (units): " + soonUnits.ToString("N0");
        }

        private void Grid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in Grid.Rows)
            {
                DataRowView view = row.DataBoundItem as DataRowView;

                if (view == null)
                {
                    continue;
                }

                int left = Int(view["days_left"]);

                if (left < 0)
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
                else if (left <= 7)
                {
                    row.DefaultCellStyle.BackColor = Color.LemonChiffon;
                }
            }
        }

        // Stock OUT for the whole remaining quantity of one batch.
        // Stock without a batch is written off without an expiry date, so it is
        // deducted from the product's untracked stock and leaves the list.
        private static void WriteOff(DataRow r)
        {
            string batch = r["batch_no"] == DBNull.Value ? null : Convert.ToString(r["batch_no"]);

            DateTime? expiry = batch == null
                ? (DateTime?)null
                : Convert.ToDateTime(r["expiry_date"]);

            InventoryData.AddMovement(
                Convert.ToString(r["product_id"]),
                false,
                "Expired",
                Int(r["remaining"]),
                batch,
                expiry,
                null,
                null,
                "Expired write-off");
        }

        private void btnWriteOffSelected_Click(object sender, EventArgs e)
        {
            DataRow r = SelectedRow();

            if (r == null)
            {
                MessageBox.Show("Select a batch in the list first.", "Write off",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool expired = Int(r["days_left"]) < 0;

            string question = "Write off " + Int(r["remaining"]) + " unit(s) of \""
                + Convert.ToString(r["product_name"]) + "\" (batch "
                + (r["batch_no"] == DBNull.Value ? "-" : Convert.ToString(r["batch_no"])) + ")?\n\n"
                + "This removes them from stock.";

            if (!expired)
            {
                question += "\n\nNote: this batch has NOT expired yet.";
            }

            if (MessageBox.Show(question, "Write off", MessageBoxButtons.YesNo,
                    expired ? MessageBoxIcon.Question : MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                WriteOff(r);
                Reload();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Write off", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnWriteOffAll_Click(object sender, EventArgs e)
        {
            DataTable dt = Grid.DataSource as DataTable;

            if (dt == null)
            {
                return;
            }

            System.Collections.Generic.List<DataRow> expired = new System.Collections.Generic.List<DataRow>();
            int units = 0;

            foreach (DataRow r in dt.Rows)
            {
                if (Int(r["days_left"]) < 0)
                {
                    expired.Add(r);
                    units += Int(r["remaining"]);
                }
            }

            if (expired.Count == 0)
            {
                MessageBox.Show("There are no expired batches in the list.", "Write off",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(
                    "Write off " + expired.Count + " expired batch(es), " + units.ToString("N0") + " unit(s) in total?\n\n"
                    + "This removes them from stock.",
                    "Write off all expired", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            int done = 0;
            string failed = "";

            foreach (DataRow r in expired)
            {
                try
                {
                    WriteOff(r);
                    done++;
                }
                catch (Exception ex)
                {
                    failed += "\n- " + Convert.ToString(r["product_name"]) + ": " + ex.Message;
                }
            }

            Reload();

            if (failed.Length > 0)
            {
                MessageBox.Show("Written off: " + done + " batch(es).\n\nNot written off:" + failed,
                    "Write off", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("Written off " + done + " batch(es).", "Write off",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}