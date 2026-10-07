using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace inventory_management_system_with_POS
{
    // Stock in (Restock) or stock out (Damaged / Expired / Adjustment)
    // for one product. Writes one row to tbl_stock_movement.
    public class frmStockMovementDialog : Form
    {
        private readonly string _productId;

        private ComboBox cmbType;
        private NumericUpDown numQty;
        private TextBox txtBatch;
        private CheckBox chkExpiry;
        private DateTimePicker dtpExpiry;
        private NumericUpDown numCost;
        private ComboBox cmbSupplier;
        private TextBox txtRemarks;
        private Button btnOk;
        private Button btnCancel;

        public frmStockMovementDialog(string productId, string productName, int currentStock, decimal defaultCost)
            : this(productId, productName, currentStock, defaultCost, 0)
        {
        }

        // initialType: 0 = Stock in, 1 = Damaged, 2 = Expired, 3 = Adjustment
        public frmStockMovementDialog(string productId, string productName, int currentStock, decimal defaultCost, int initialType)
        {
            _productId = productId;

            Text = "Stock In / Out";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(440, 392);

            Label header = new Label();
            header.Text = productName + "  (current stock: " + currentStock + ")";
            header.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            header.SetBounds(15, 12, 410, 24);
            Controls.Add(header);

            int y = 48;

            cmbType = new ComboBox();
            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbType.Items.AddRange(new object[]
            {
                "Stock in (Restock)",
                "Stock out - Damaged",
                "Stock out - Expired",
                "Stock out - Adjustment"
            });
            cmbType.SelectedIndex = 0;
            cmbType.SelectedIndexChanged += cmbType_SelectedIndexChanged;
            AddRow("Type:", cmbType, ref y);

            numQty = new NumericUpDown();
            numQty.Minimum = 1;
            numQty.Maximum = 1000000;
            numQty.Value = 1;
            AddRow("Quantity:", numQty, ref y);

            txtBatch = new TextBox();
            txtBatch.MaxLength = 50;
            AddRow("Batch no.:", txtBatch, ref y);

            chkExpiry = new CheckBox();
            chkExpiry.Text = "Has expiry date";
            chkExpiry.CheckedChanged += delegate { dtpExpiry.Enabled = chkExpiry.Checked; };
            AddRow("", chkExpiry, ref y);

            dtpExpiry = new DateTimePicker();
            dtpExpiry.Format = DateTimePickerFormat.Short;
            dtpExpiry.Value = DateTime.Today.AddMonths(6);
            dtpExpiry.Enabled = false;
            AddRow("Expiry date:", dtpExpiry, ref y);

            numCost = new NumericUpDown();
            numCost.DecimalPlaces = 2;
            numCost.Minimum = 0;
            numCost.Maximum = 100000000;
            numCost.Value = defaultCost < 0 ? 0 : Math.Min(defaultCost, numCost.Maximum);
            AddRow("Unit cost:", numCost, ref y);

            cmbSupplier = new ComboBox();
            cmbSupplier.DropDownStyle = ComboBoxStyle.DropDownList;
            AddRow("Supplier:", cmbSupplier, ref y);

            txtRemarks = new TextBox();
            txtRemarks.MaxLength = 255;
            AddRow("Remarks:", txtRemarks, ref y);

            btnOk = new Button();
            btnOk.Text = "Save";
            btnOk.SetBounds(235, 345, 90, 32);
            btnOk.Click += btnOk_Click;
            Controls.Add(btnOk);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.SetBounds(335, 345, 90, 32);
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;

            LoadSuppliers();

            if (initialType > 0 && initialType < cmbType.Items.Count)
            {
                cmbType.SelectedIndex = initialType;
            }
        }

        private void AddRow(string label, Control input, ref int y)
        {
            Label l = new Label();
            l.Text = label;
            l.SetBounds(15, y + 3, 110, 22);
            Controls.Add(l);

            input.SetBounds(130, y, 295, 26);
            Controls.Add(input);

            y += 36;
        }

        private void LoadSuppliers()
        {
            try
            {
                DataTable dt = InventoryData.GetSuppliers();

                DataRow none = dt.NewRow();
                none["supplier_id"] = "";
                none["supplier_name"] = "(none)";
                dt.Rows.InsertAt(none, 0);

                cmbSupplier.DataSource = dt;
                cmbSupplier.DisplayMember = "supplier_name";
                cmbSupplier.ValueMember = "supplier_id";
                cmbSupplier.SelectedIndex = 0;
            }
            catch (Exception)
            {
                // Supplier is optional; the dialog still works without the list.
            }
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isIn = cmbType.SelectedIndex == 0;

            numCost.Enabled = isIn;
            cmbSupplier.Enabled = isIn;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            bool isIn = cmbType.SelectedIndex == 0;
            string reason;

            switch (cmbType.SelectedIndex)
            {
                case 1: reason = "Damaged"; break;
                case 2: reason = "Expired"; break;
                case 3: reason = "Adjustment"; break;
                default: reason = "Restock"; break;
            }

            DateTime? expiry = null;

            if (chkExpiry.Checked)
            {
                expiry = dtpExpiry.Value.Date;
            }

            decimal? cost = null;
            string supplier = null;

            if (isIn)
            {
                cost = numCost.Value;

                if (cmbSupplier.SelectedValue != null)
                {
                    supplier = Convert.ToString(cmbSupplier.SelectedValue);
                }
            }

            try
            {
                InventoryData.AddMovement(
                    _productId, isIn, reason, (int)numQty.Value,
                    txtBatch.Text, expiry, cost, supplier, txtRemarks.Text);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Search box + product list. Used by the Damage Report to choose
    // which product to report. Read ProductId / ProductName / Stock / Cost
    // after ShowDialog returns DialogResult.OK.
    public class frmProductPickDialog : Form
    {
        private readonly TextBox _txtSearch;
        private readonly DataGridView _grid;

        public string ProductId = "";
        public string ProductName = "";
        public int Stock;
        public decimal Cost;

        public frmProductPickDialog()
        {
            Text = "Choose product";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(640, 420);
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            _grid = new DataGridView();
            _grid.Dock = DockStyle.Fill;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.MultiSelect = false;
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.BackgroundColor = Color.White;
            _grid.CellDoubleClick += delegate (object s, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0)
                {
                    Accept();
                }
            };

            Panel top = new Panel();
            top.Dock = DockStyle.Top;
            top.Height = 42;

            Label lbl = new Label();
            lbl.Text = "Search:";
            lbl.SetBounds(12, 12, 55, 20);
            top.Controls.Add(lbl);

            _txtSearch = new TextBox();
            _txtSearch.SetBounds(70, 9, 340, 25);
            _txtSearch.TextChanged += delegate { LoadProducts(); };
            top.Controls.Add(_txtSearch);

            Panel bottom = new Panel();
            bottom.Dock = DockStyle.Bottom;
            bottom.Height = 50;

            Button ok = new Button();
            ok.Text = "Select";
            ok.SetBounds(430, 8, 90, 32);
            ok.Click += delegate { Accept(); };
            bottom.Controls.Add(ok);

            Button cancel = new Button();
            cancel.Text = "Cancel";
            cancel.SetBounds(530, 8, 90, 32);
            cancel.DialogResult = DialogResult.Cancel;
            bottom.Controls.Add(cancel);

            // Fill first, then the outer bars.
            Controls.Add(_grid);
            Controls.Add(bottom);
            Controls.Add(top);

            AcceptButton = ok;
            CancelButton = cancel;

            Shown += delegate
            {
                LoadProducts();
                _txtSearch.Focus();
            };
        }

        private void LoadProducts()
        {
            try
            {
                _grid.DataSource = InventoryData.GetProducts(_txtSearch.Text);

                string[] keep = new string[] { "product_id", "product_name", "category_name", "stock" };

                foreach (DataGridViewColumn c in _grid.Columns)
                {
                    c.Visible = Array.IndexOf(keep, c.Name) >= 0;
                }

                if (_grid.Columns.Contains("product_id")) { _grid.Columns["product_id"].HeaderText = "ID"; }
                if (_grid.Columns.Contains("product_name")) { _grid.Columns["product_name"].HeaderText = "Product"; }
                if (_grid.Columns.Contains("category_name")) { _grid.Columns["category_name"].HeaderText = "Category"; }
                if (_grid.Columns.Contains("stock")) { _grid.Columns["stock"].HeaderText = "In stock"; }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Accept()
        {
            DataRowView view = _grid.CurrentRow == null ? null : _grid.CurrentRow.DataBoundItem as DataRowView;

            if (view == null)
            {
                MessageBox.Show("Select a product first.", "Choose product",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ProductId = Convert.ToString(view["product_id"]);
            ProductName = Convert.ToString(view["product_name"]);
            Stock = Convert.ToInt32(view["stock"]);
            Cost = Convert.ToDecimal(view["cost_price"]);

            DialogResult = DialogResult.OK;
            Close();
        }
    }

    // Read-only list of every stock movement of one product.
    public class frmStockHistoryDialog : Form
    {
        public frmStockHistoryDialog(string productId, string productName)
        {
            Text = "Stock history - " + productName;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(900, 420);

            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            Controls.Add(grid);

            try
            {
                grid.DataSource = InventoryData.GetHistory(productId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Shows the receipt text and lets the cashier print it.
    public class frmReceiptDialog : Form
    {
        private readonly string _text;
        private readonly TextBox _box;

        public frmReceiptDialog(string receiptText)
        {
            _text = receiptText;

            Text = "Receipt";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(380, 520);

            _box = new TextBox();
            _box.Multiline = true;
            _box.ReadOnly = true;
            _box.ScrollBars = ScrollBars.Vertical;
            _box.Font = new Font("Consolas", 10F);
            _box.Text = receiptText;
            _box.SetBounds(10, 10, 360, 450);
            _box.SelectionStart = 0;
            _box.SelectionLength = 0;
            Controls.Add(_box);

            Button print = new Button();
            print.Text = "Print";
            print.SetBounds(190, 472, 85, 34);
            print.Click += print_Click;
            Controls.Add(print);

            Button close = new Button();
            close.Text = "Close";
            close.SetBounds(285, 472, 85, 34);
            close.DialogResult = DialogResult.Cancel;
            Controls.Add(close);

            CancelButton = close;
        }

        private void print_Click(object sender, EventArgs e)
        {
            try
            {
                using (PrintDocument doc = new PrintDocument())
                using (PrintDialog dlg = new PrintDialog())
                {
                    doc.PrintPage += delegate (object s, PrintPageEventArgs args)
                    {
                        using (Font font = new Font("Consolas", 9F))
                        {
                            args.Graphics.DrawString(_text, font, Brushes.Black, 20, 20);
                        }
                    };

                    dlg.Document = doc;

                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        doc.Print();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Print", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
