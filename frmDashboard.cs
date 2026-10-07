using System;
using System.Drawing;
using System.Windows.Forms;

namespace inventory_management_system_with_POS
{
    public partial class frmDashboard : Form
    {
        // Sales by Category
        private Label lblSalesByCategory;
        private Label lblSnacks;
        private Label lblBeverage;
        private Label lblHousehold;
        private Label lblCancelled;

        private TextBox txtSnacks;
        private TextBox txtBeverage;
        private TextBox txtHousehold;
        private TextBox txtCancelled;

        // Date Range
        private Label lblDateRange;
        private Label lblFrom;
        private Label lblTo;

        private TextBox txtFrom;
        private TextBox txtTo;

        // Search
        private Label lblSearch;
        private TextBox txtSearch;

        // DataGridView
        private DataGridView dgvSales;

        public frmDashboard()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblSalesByCategory = new System.Windows.Forms.Label();
            this.lblSnacks = new System.Windows.Forms.Label();
            this.lblBeverage = new System.Windows.Forms.Label();
            this.lblHousehold = new System.Windows.Forms.Label();
            this.lblCancelled = new System.Windows.Forms.Label();
            this.txtSnacks = new System.Windows.Forms.TextBox();
            this.txtBeverage = new System.Windows.Forms.TextBox();
            this.txtHousehold = new System.Windows.Forms.TextBox();
            this.txtCancelled = new System.Windows.Forms.TextBox();
            this.lblDateRange = new System.Windows.Forms.Label();
            this.lblFrom = new System.Windows.Forms.Label();
            this.lblTo = new System.Windows.Forms.Label();
            this.txtFrom = new System.Windows.Forms.TextBox();
            this.txtTo = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.dgvSales = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSales)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblSalesByCategory
            // 
            this.lblSalesByCategory.BackColor = System.Drawing.Color.MistyRose;
            this.lblSalesByCategory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblSalesByCategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblSalesByCategory.Location = new System.Drawing.Point(28, 74);
            this.lblSalesByCategory.Name = "lblSalesByCategory";
            this.lblSalesByCategory.Size = new System.Drawing.Size(236, 51);
            this.lblSalesByCategory.TabIndex = 0;
            this.lblSalesByCategory.Text = "SALES BY CATEGORY:";
            this.lblSalesByCategory.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSnacks
            // 
            this.lblSnacks.AutoSize = true;
            this.lblSnacks.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblSnacks.Location = new System.Drawing.Point(270, 125);
            this.lblSnacks.Name = "lblSnacks";
            this.lblSnacks.Size = new System.Drawing.Size(84, 25);
            this.lblSnacks.TabIndex = 1;
            this.lblSnacks.Text = "Snacks:";
            // 
            // lblBeverage
            // 
            this.lblBeverage.AutoSize = true;
            this.lblBeverage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblBeverage.Location = new System.Drawing.Point(270, 169);
            this.lblBeverage.Name = "lblBeverage";
            this.lblBeverage.Size = new System.Drawing.Size(102, 25);
            this.lblBeverage.TabIndex = 3;
            this.lblBeverage.Text = "Beverage:";
            // 
            // lblHousehold
            // 
            this.lblHousehold.AutoSize = true;
            this.lblHousehold.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblHousehold.Location = new System.Drawing.Point(270, 216);
            this.lblHousehold.Name = "lblHousehold";
            this.lblHousehold.Size = new System.Drawing.Size(112, 25);
            this.lblHousehold.TabIndex = 5;
            this.lblHousehold.Text = "Household:";
            // 
            // lblCancelled
            // 
            this.lblCancelled.AutoSize = true;
            this.lblCancelled.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblCancelled.Location = new System.Drawing.Point(270, 260);
            this.lblCancelled.Name = "lblCancelled";
            this.lblCancelled.Size = new System.Drawing.Size(106, 25);
            this.lblCancelled.TabIndex = 7;
            this.lblCancelled.Text = "Cancelled:";
            // 
            // txtSnacks
            // 
            this.txtSnacks.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtSnacks.Location = new System.Drawing.Point(384, 122);
            this.txtSnacks.Name = "txtSnacks";
            this.txtSnacks.ReadOnly = true;
            this.txtSnacks.Size = new System.Drawing.Size(247, 28);
            this.txtSnacks.TabIndex = 2;
            // 
            // txtBeverage
            // 
            this.txtBeverage.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtBeverage.Location = new System.Drawing.Point(384, 169);
            this.txtBeverage.Name = "txtBeverage";
            this.txtBeverage.ReadOnly = true;
            this.txtBeverage.Size = new System.Drawing.Size(247, 28);
            this.txtBeverage.TabIndex = 4;
            // 
            // txtHousehold
            // 
            this.txtHousehold.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtHousehold.Location = new System.Drawing.Point(384, 215);
            this.txtHousehold.Name = "txtHousehold";
            this.txtHousehold.ReadOnly = true;
            this.txtHousehold.Size = new System.Drawing.Size(247, 28);
            this.txtHousehold.TabIndex = 6;
            // 
            // txtCancelled
            // 
            this.txtCancelled.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtCancelled.Location = new System.Drawing.Point(384, 262);
            this.txtCancelled.Name = "txtCancelled";
            this.txtCancelled.ReadOnly = true;
            this.txtCancelled.Size = new System.Drawing.Size(247, 28);
            this.txtCancelled.TabIndex = 8;
            // 
            // lblDateRange
            // 
            this.lblDateRange.BackColor = System.Drawing.Color.MistyRose;
            this.lblDateRange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDateRange.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblDateRange.Location = new System.Drawing.Point(657, 74);
            this.lblDateRange.Name = "lblDateRange";
            this.lblDateRange.Size = new System.Drawing.Size(196, 51);
            this.lblDateRange.TabIndex = 9;
            this.lblDateRange.Text = "Date Range:";
            this.lblDateRange.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblFrom.Location = new System.Drawing.Point(790, 151);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(63, 25);
            this.lblFrom.TabIndex = 10;
            this.lblFrom.Text = "From:";
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblTo.Location = new System.Drawing.Point(790, 230);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(42, 25);
            this.lblTo.TabIndex = 12;
            this.lblTo.Text = "To:";
            // 
            // txtFrom
            // 
            this.txtFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtFrom.Location = new System.Drawing.Point(837, 176);
            this.txtFrom.Name = "txtFrom";
            this.txtFrom.Size = new System.Drawing.Size(235, 28);
            this.txtFrom.TabIndex = 11;
            // 
            // txtTo
            // 
            this.txtTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtTo.Location = new System.Drawing.Point(837, 257);
            this.txtTo.Name = "txtTo";
            this.txtTo.Size = new System.Drawing.Size(235, 28);
            this.txtTo.TabIndex = 13;
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblSearch.Location = new System.Drawing.Point(34, 340);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(81, 25);
            this.lblSearch.TabIndex = 14;
            this.lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtSearch.Location = new System.Drawing.Point(137, 334);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(458, 28);
            this.txtSearch.TabIndex = 15;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // dgvSales
            // 
            this.dgvSales.AllowUserToDeleteRows = false;
            this.dgvSales.AllowUserToResizeRows = false;
            this.dgvSales.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSales.BackgroundColor = System.Drawing.Color.DarkGray;
            this.dgvSales.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSales.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn9,
            this.dataGridViewTextBoxColumn10,
            this.dataGridViewTextBoxColumn11,
            this.dataGridViewTextBoxColumn12});
            this.dgvSales.Location = new System.Drawing.Point(137, 386);
            this.dgvSales.MultiSelect = false;
            this.dgvSales.Name = "dgvSales";
            this.dgvSales.RowHeadersWidth = 51;
            this.dgvSales.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSales.Size = new System.Drawing.Size(935, 291);
            this.dgvSales.TabIndex = 16;
            // 
            // dataGridViewTextBoxColumn9
            // 
            this.dataGridViewTextBoxColumn9.HeaderText = "Receipt No.";
            this.dataGridViewTextBoxColumn9.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            // 
            // dataGridViewTextBoxColumn10
            // 
            this.dataGridViewTextBoxColumn10.HeaderText = "Item Count";
            this.dataGridViewTextBoxColumn10.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
            // 
            // dataGridViewTextBoxColumn11
            // 
            this.dataGridViewTextBoxColumn11.HeaderText = "Total";
            this.dataGridViewTextBoxColumn11.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            // 
            // dataGridViewTextBoxColumn12
            // 
            this.dataGridViewTextBoxColumn12.HeaderText = "Time";
            this.dataGridViewTextBoxColumn12.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1150, 55);
            this.panel1.TabIndex = 19;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(13, 25);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(133, 22);
            this.label8.TabIndex = 19;
            this.label8.Text = "DASHBOARD";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::inventory_management_system_with_POS.Properties.Resources.button__3_1;
            this.pictureBox1.Location = new System.Drawing.Point(1103, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(44, 47);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 18;
            this.pictureBox1.TabStop = false;
            // 
            // frmDashboard
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(1150, 713);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblSalesByCategory);
            this.Controls.Add(this.lblSnacks);
            this.Controls.Add(this.txtSnacks);
            this.Controls.Add(this.lblBeverage);
            this.Controls.Add(this.txtBeverage);
            this.Controls.Add(this.lblHousehold);
            this.Controls.Add(this.txtHousehold);
            this.Controls.Add(this.lblCancelled);
            this.Controls.Add(this.txtCancelled);
            this.Controls.Add(this.lblDateRange);
            this.Controls.Add(this.lblFrom);
            this.Controls.Add(this.txtFrom);
            this.Controls.Add(this.lblTo);
            this.Controls.Add(this.txtTo);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.dgvSales);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.Name = "frmDashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmDashboard";
            this.Load += new System.EventHandler(this.frmDashboard_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSales)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        // =====================================================
        // SEARCH
        // =====================================================

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim();

            foreach (DataGridViewRow row in dgvSales.Rows)
            {
                if (row.IsNewRow)
                    continue;

                bool found = false;

                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null &&
                        cell.Value.ToString()
                        .IndexOf(search,
                        StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        found = true;
                        break;
                    }
                }

                row.Visible = found;
            }
        }

        private void frmDashboard_Load(object sender, EventArgs e)
        {

        }

        private void frmDashboard_Load_1(object sender, EventArgs e)
        {
            txtSnacks.ReadOnly = true;
            txtBeverage.ReadOnly = true;
            txtHousehold.ReadOnly = true;
            txtCancelled.ReadOnly = true;

            // Dates are typed as yyyy-MM-dd. Default = today.
            txtFrom.Text = DateTime.Today.ToString("yyyy-MM-dd");
            txtTo.Text = DateTime.Today.ToString("yyyy-MM-dd");

            ToolTip tip = new ToolTip();
            tip.SetToolTip(txtFrom, "yyyy-MM-dd  (press Enter to refresh)");
            tip.SetToolTip(txtTo, "yyyy-MM-dd  (press Enter to refresh)");

            txtFrom.Leave += delegate { LoadDashboard(); };
            txtTo.Leave += delegate { LoadDashboard(); };
            txtFrom.KeyDown += DateBox_KeyDown;
            txtTo.KeyDown += DateBox_KeyDown;

            LoadDashboard();
        }

        private void DateBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                LoadDashboard();
            }
        }

        // =====================================================
        // DATA  (tbl_sale / tbl_sale_item / tbl_product / tbl_category)
        // =====================================================
        private void LoadDashboard()
        {
            DateTime from;
            DateTime to;

            if (!DateTime.TryParseExact(txtFrom.Text.Trim(), "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out from) ||
                !DateTime.TryParseExact(txtTo.Text.Trim(), "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out to))
            {
                MessageBox.Show("Please type the dates as yyyy-MM-dd, for example 2026-10-07.",
                    "Date range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (to < from)
            {
                MessageBox.Show("\"To\" cannot be earlier than \"From\".",
                    "Date range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const string peso = "\u20B1";

            try
            {
                // Sales list
                System.Data.DataTable sales = InventoryData.GetSales(from, to);

                dgvSales.Rows.Clear();

                foreach (System.Data.DataRow r in sales.Rows)
                {
                    dgvSales.Rows.Add(
                        r["sale_id"].ToString(),
                        Convert.ToInt32(r["item_count"]).ToString(),
                        peso + Convert.ToDecimal(r["total_amount"]).ToString("N2"),
                        Convert.ToDateTime(r["created_at"]).ToString("yyyy-MM-dd hh:mm tt"));
                }

                // The three best-selling categories fill the three boxes.
                System.Data.DataTable cats = InventoryData.GetSalesByCategory(from, to);

                Label[] labels = new Label[] { lblSnacks, lblBeverage, lblHousehold };
                TextBox[] boxes = new TextBox[] { txtSnacks, txtBeverage, txtHousehold };

                for (int i = 0; i < boxes.Length; i++)
                {
                    if (i < cats.Rows.Count)
                    {
                        labels[i].Text = cats.Rows[i]["category_name"].ToString() + ":";
                        boxes[i].Text = peso + Convert.ToDecimal(cats.Rows[i]["total"]).ToString("N2");
                    }
                    else
                    {
                        labels[i].Text = "-";
                        boxes[i].Text = "";
                    }
                }

                txtCancelled.Text = peso + InventoryData.GetCancelledTotal(from, to).ToString("N2");

                txtSearch_TextChanged(txtSearch, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}