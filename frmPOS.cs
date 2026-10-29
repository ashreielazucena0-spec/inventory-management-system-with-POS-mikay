using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace inventory_management_system_with_POS
{
    // ===================== FrmPOS =====================
    public partial class FrmPOS : Form
    {
        // Set once from frmMainform:  FrmPOS.DefaultConnection = _db.conn;
        public static MySqlConnection DefaultConnection { get; set; }

        // Philippine VAT rate (prices in the database are treated as VAT-INCLUSIVE)
        private const decimal VatRate = 0.12m;

        private MySqlConnection _conn;
        private bool _layoutReady;

        // found automatically from your Designer layout (no renaming needed)
        private TextBoxBase txtSearchProduct;
        private TextBoxBase txtSearchOrder;
        private TextBoxBase txtCashTendered;

        private Button btnAddTo;
        private Button btnRemove;
        private Button btnClear;
        private Button btnSeniorCitizen;
        private Button btnPayAndPrint;

        private Label lblProductHeader;
        private Label lblOrderHeader;
        private Label lblCashCaption;

        // created in code, shown beside Cash Tendered
        private Label lblSubtotal;
        private Label lblDiscount;
        private Label lblVat;
        private Label lblTotal;
        private Label lblChange;

        private bool _seniorOn;
        private decimal _subtotal;      // sum of the item prices (VAT included)
        private decimal _discount;      // senior citizen 20% discount
        private decimal _vatable;       // VATable sales (price without VAT)
        private decimal _vat;           // 12% VAT contained in the total
        private decimal _vatExempt;     // VAT-exempt sales (senior citizen)
        private decimal _vatRemoved;    // VAT taken off because of VAT exemption
        private decimal _total;
        private List<string> _receiptLines = new List<string>();

        private class OrderLine
        {
            public string ProductId;
            public string Name;
            public decimal Price;
            public int Qty;
            public decimal Total;
        }

        public FrmPOS()
        {
            InitializeComponent();
            _conn = DefaultConnection;

            Load += (s, e) => SetupPos();
            Resize += (s, e) => ArrangeLayout();
        }

        // Optional: pass the connection directly -> new FrmPOS(_db.conn)
        public FrmPOS(MySqlConnection conn) : this()
        {
            if (conn != null)
            {
                _conn = conn;
            }
        }

        // ---- handlers created by your Designer (keep them) ----
        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void FrmPOS_Load(object sender, EventArgs e)
        {
        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        // ---------------------------------------------------------
        // SETUP
        // ---------------------------------------------------------
        private void SetupPos()
        {
            SetupGrids();
            FindDesignerControls();
            CreateTotalLabels();
            WireEvents();
            PinControls();

            _layoutReady = true;
            ArrangeLayout();
            RecalculateTotals();

            if (_conn == null)
            {
                MessageBox.Show(
                    "No database connection was given to this form.\n\n" +
                    "In frmMainform add this line after the database is ready:\n" +
                    "FrmPOS.DefaultConnection = _db.conn;",
                    "Prime Mart Pos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadProducts("");
        }

        private void SetupGrids()
        {
            // dataGridView1 = PRODUCT LIST
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("ProductId", "ProductId");
            dataGridView1.Columns.Add("Barcode", "Barcode");
            dataGridView1.Columns.Add("Product", "Product");
            dataGridView1.Columns.Add("Price", "Price");
            dataGridView1.Columns.Add("Stocks", "Stocks");

            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.MultiSelect = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.Columns["ProductId"].Visible = false;
            dataGridView1.Columns["Barcode"].FillWeight = 80;
            dataGridView1.Columns["Product"].FillWeight = 500;
            dataGridView1.Columns["Price"].FillWeight = 80;
            dataGridView1.Columns["Stocks"].FillWeight = 80;
            dataGridView1.Columns["Price"].DefaultCellStyle.Format = "N2";

            // dataGridView2 = CURRENT ORDER
            dataGridView2.Columns.Clear();
            dataGridView2.Columns.Add("ProductId", "ProductId");
            dataGridView2.Columns.Add("Stock", "Stock");
            dataGridView2.Columns.Add("Product", "Product");
            dataGridView2.Columns.Add("Price", "Price");
            dataGridView2.Columns.Add("Qty", "Qty");
            dataGridView2.Columns.Add("Total", "Total");

            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.MultiSelect = false;
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView2.Columns["ProductId"].Visible = false;
            dataGridView2.Columns["Stock"].Visible = false;
            dataGridView2.Columns["Product"].FillWeight = 200;
            dataGridView2.Columns["Price"].FillWeight = 80;
            dataGridView2.Columns["Qty"].FillWeight = 50;
            dataGridView2.Columns["Total"].FillWeight = 90;
            dataGridView2.Columns["Price"].DefaultCellStyle.Format = "N2";
            dataGridView2.Columns["Total"].DefaultCellStyle.Format = "N2";

            // only the Qty column can be typed in
            dataGridView2.ReadOnly = false;
            foreach (DataGridViewColumn col in dataGridView2.Columns)
            {
                col.ReadOnly = col.Name != "Qty";
            }
        }

        private IEnumerable<Control> AllControls(Control root)
        {
            foreach (Control c in root.Controls)
            {
                yield return c;

                foreach (Control child in AllControls(c))
                {
                    yield return child;
                }
            }
        }

        private Button FindButton(string text)
        {
            foreach (Control c in AllControls(this))
            {
                Button b = c as Button;

                if (b != null && string.Equals(b.Text.Trim(), text, StringComparison.OrdinalIgnoreCase))
                {
                    return b;
                }
            }

            return null;
        }

        private Label FindLabel(string text)
        {
            foreach (Control c in AllControls(this))
            {
                Label l = c as Label;

                if (l != null && string.Equals(l.Text.Trim(), text, StringComparison.OrdinalIgnoreCase))
                {
                    return l;
                }
            }

            return null;
        }

        // Buttons/labels are found by their text, text boxes by their position:
        // two boxes on top (left = product search, right = order search),
        // and the lowest box = Cash Tendered.
        private void FindDesignerControls()
        {
            btnAddTo = FindButton("ADD TO");
            btnRemove = FindButton("REM");
            btnClear = FindButton("CLEAR");
            btnSeniorCitizen = FindButton("SENIOR CITIZEN");
            btnPayAndPrint = FindButton("Pay and Print");

            // show the shortcut keys on the buttons (2 lines so the text always fits)
            if (btnAddTo != null) btnAddTo.Text = "ADD TO\n[Alt+A]";
            if (btnRemove != null) btnRemove.Text = "REM\n[Alt+R]";
            if (btnClear != null) btnClear.Text = "CLEAR\n[Alt+C]";
            if (btnSeniorCitizen != null) btnSeniorCitizen.Text = "SENIOR CITIZEN\n[Alt+S]";
            if (btnPayAndPrint != null) btnPayAndPrint.Text = "Pay and Print\n[Alt+P]";

            lblProductHeader = FindLabel("PRODUCT LIST");
            lblOrderHeader = FindLabel("CURRENT ORDER");
            lblCashCaption = FindLabel("Cash Tendered");

            List<TextBoxBase> boxes = new List<TextBoxBase>();

            foreach (Control c in AllControls(this))
            {
                MaskedTextBox m = c as MaskedTextBox;
                if (m != null)
                {
                    m.Mask = "";   // a mask would block normal typing
                }

                TextBoxBase t = c as TextBoxBase;
                if (t != null)
                {
                    boxes.Add(t);
                }
            }

            if (boxes.Count >= 3)
            {
                boxes.Sort((a, b) => a.Top.CompareTo(b.Top));
                txtCashTendered = boxes[boxes.Count - 1];
                boxes.RemoveAt(boxes.Count - 1);

                boxes.Sort((a, b) => a.Left.CompareTo(b.Left));
                txtSearchProduct = boxes[0];
                txtSearchOrder = boxes[1];
            }
        }

        private Label MakeLabel(Control host, int left, int top, int width, int height, float fontSize, string text)
        {
            Label lbl = new Label
            {
                Text = text,
                AutoSize = false,
                Location = new Point(left, top),
                Size = new Size(width, height),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Arial", fontSize, FontStyle.Bold)
            };

            host.Controls.Add(lbl);
            return lbl;
        }

        private void CreateTotalLabels()
        {
            if (txtCashTendered == null || txtCashTendered.Parent == null) return;

            Control host = txtCashTendered.Parent;

            // make room beside the cash box
            txtCashTendered.Width = Math.Min(txtCashTendered.Width, 250);
            txtCashTendered.Font = new Font("Arial", 12F);

            int top = txtCashTendered.Top;
            int h = Math.Max(txtCashTendered.Height, 28);
            int x = txtCashTendered.Right + 15;

            lblSubtotal = MakeLabel(host, x, top, 170, h, 10F, "Subtotal: 0.00");
            x += 175;
            lblDiscount = MakeLabel(host, x, top, 170, h, 10F, "Discount: 0.00");
            x += 175;
            lblVat = MakeLabel(host, x, top, 200, h, 10F, "VAT 12%: 0.00");
            x += 205;
            lblTotal = MakeLabel(host, x, top, 200, h, 12F, "TOTAL: 0.00");
            x += 205;
            lblChange = MakeLabel(host, x, top, 200, h, 12F, "Change: 0.00");
        }

        private static void Pin(Control c)
        {
            if (c != null)
            {
                c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            }
        }

        // we position everything ourselves, so remove designer anchors
        private void PinControls()
        {
            Pin(dataGridView1);
            Pin(dataGridView2);
            Pin(txtSearchProduct);
            Pin(txtSearchOrder);
            Pin(txtCashTendered);
            Pin(btnAddTo);
            Pin(btnRemove);
            Pin(btnClear);
            Pin(btnSeniorCitizen);
            Pin(btnPayAndPrint);
            Pin(lblProductHeader);
            Pin(lblOrderHeader);
            Pin(lblCashCaption);
            Pin(lblSubtotal);
            Pin(lblDiscount);
            Pin(lblVat);
            Pin(lblTotal);
            Pin(lblChange);
        }

        // Fits everything inside the window, so nothing is cut off
        private void ArrangeLayout()
        {
            if (!_layoutReady) return;

            int margin = 10;
            int gap = 7;
            int availableWidth = ClientSize.Width - margin * 2 - gap;
            if (availableWidth < 300) return;

            int leftWidth = (int)(availableWidth * 0.62);
            int rightWidth = availableWidth - leftWidth;
            int rightX = margin + leftWidth + gap;

            if (txtSearchProduct != null)
            {
                txtSearchProduct.Left = margin;
                txtSearchProduct.Width = leftWidth;
            }

            if (txtSearchOrder != null)
            {
                txtSearchOrder.Left = rightX;
                txtSearchOrder.Width = rightWidth;
            }

            if (lblProductHeader != null)
            {
                lblProductHeader.Left = margin;
                lblProductHeader.Width = leftWidth;
            }

            if (lblOrderHeader != null)
            {
                lblOrderHeader.Left = rightX;
                lblOrderHeader.Width = rightWidth;
            }

            int gridTop = dataGridView1.Top;
            int btnH = btnAddTo != null ? btnAddTo.Height : 47;
            int rowH = txtCashTendered != null ? Math.Max(txtCashTendered.Height, 28) : 30;
            int footer = 15 + btnH + 12 + rowH + 15;
            int gridHeight = Math.Max(120, ClientSize.Height - gridTop - footer);

            dataGridView1.SetBounds(margin, gridTop, leftWidth, gridHeight);
            dataGridView2.SetBounds(rightX, gridTop, rightWidth, gridHeight);

            int buttonY = gridTop + gridHeight + 15;

            Button[] buttons = { btnAddTo, btnRemove, btnClear, btnSeniorCitizen, btnPayAndPrint };
            foreach (Button b in buttons)
            {
                if (b != null) b.Top = buttonY;
            }

            int cashY = buttonY + btnH + 12;

            if (txtCashTendered != null)
            {
                txtCashTendered.Top = cashY;
            }

            Control[] rowLabels = { lblCashCaption, lblSubtotal, lblDiscount, lblVat, lblTotal, lblChange };
            foreach (Control c in rowLabels)
            {
                if (c != null) c.Top = cashY + (rowH - c.Height) / 2;
            }
        }

        private void WireEvents()
        {
            if (btnAddTo != null) btnAddTo.Click += BtnAddTo_Click;
            if (btnRemove != null) btnRemove.Click += BtnRemove_Click;
            if (btnClear != null) btnClear.Click += BtnClear_Click;
            if (btnSeniorCitizen != null) btnSeniorCitizen.Click += BtnSeniorCitizen_Click;
            if (btnPayAndPrint != null) btnPayAndPrint.Click += BtnPayAndPrint_Click;

            if (txtSearchProduct != null)
            {
                txtSearchProduct.TextChanged += (s, e) => LoadProducts(txtSearchProduct.Text.Trim());
                txtSearchProduct.KeyDown += TxtSearchProduct_KeyDown;
            }

            if (txtSearchOrder != null)
            {
                txtSearchOrder.TextChanged += (s, e) => FilterOrder();
            }

            if (txtCashTendered != null)
            {
                txtCashTendered.TextChanged += (s, e) => UpdateChange();
                txtCashTendered.KeyPress += TxtCashTendered_KeyPress;
                txtCashTendered.KeyDown += TxtCashTendered_KeyDown;
            }

            dataGridView1.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    AddProductFromRow(dataGridView1.Rows[e.RowIndex]);
                }
            };

            dataGridView2.CellEndEdit += DataGridView2_CellEndEdit;
        }

        // ---------------------------------------------------------
        // DATABASE
        // ---------------------------------------------------------
        private void EnsureConnectionOpen()
        {
            if (_conn.State != ConnectionState.Open)
            {
                _conn.Open();
            }
        }

        private void LoadProducts(string search)
        {
            if (_conn == null) return;

            try
            {
                EnsureConnectionOpen();
                dataGridView1.Rows.Clear();

                using (MySqlCommand cmd = new MySqlCommand(PosSql.SelectProducts, _conn))
                {
                    cmd.Parameters.AddWithValue("@s", "%" + search + "%");

                    using (MySqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            dataGridView1.Rows.Add(
                                Convert.ToString(rd["product_id"]),
                                Convert.ToString(rd["barcode"]),
                                Convert.ToString(rd["product_name"]),
                                Convert.ToDecimal(rd["price"]),
                                Convert.ToInt32(rd["stock"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot load products: " + ex.Message,
                    "MIKMIK POS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Looks up the logged-in cashier's acc_id (null if it cannot be found)
        private object GetCashierId(MySqlTransaction tx)
        {
            try
            {
                if (string.IsNullOrEmpty(Session.AccountName)) return DBNull.Value;

                using (MySqlCommand cmd = new MySqlCommand(PosSql.FindCashier, _conn, tx))
                {
                    cmd.Parameters.AddWithValue("@name", Session.AccountName);
                    object r = cmd.ExecuteScalar();
                    return r == null ? DBNull.Value : r;
                }
            }
            catch
            {
                return DBNull.Value;
            }
        }

        private string SaveSale(List<OrderLine> items, decimal cash, decimal change)
        {
            EnsureConnectionOpen();

            // tbl_sale.sale_id is text, so we build the number here
            string saleId = "SL" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            using (MySqlTransaction tx = _conn.BeginTransaction())
            {
                try
                {
                    object cashier = GetCashierId(tx);

                    using (MySqlCommand cmd = new MySqlCommand(PosSql.InsertSale, _conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", saleId);
                        cmd.Parameters.AddWithValue("@cashier", cashier);
                        cmd.Parameters.AddWithValue("@subtotal", _subtotal);
                        cmd.Parameters.AddWithValue("@discount", _discount);
                        cmd.Parameters.AddWithValue("@tax", _vat);          // 12% VAT contained in the total
                        cmd.Parameters.AddWithValue("@total", _total);
                        cmd.Parameters.AddWithValue("@paid", cash);
                        cmd.Parameters.AddWithValue("@change", change);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (OrderLine it in items)
                    {
                        // 1) lock the product row so two cashiers cannot sell the same last item
                        using (MySqlCommand cmd = new MySqlCommand(PosSql.LockProduct, _conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@prod", it.ProductId);
                            cmd.ExecuteScalar();
                        }

                        // 2) stock = IN movements minus OUT movements
                        decimal available;
                        using (MySqlCommand cmd = new MySqlCommand(PosSql.StockOf, _conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@prod", it.ProductId);
                            available = Convert.ToDecimal(cmd.ExecuteScalar());
                        }

                        if (available < it.Qty)
                        {
                            throw new InvalidOperationException(
                                "Not enough stock for " + it.Name + ". Available: " + available.ToString("0"));
                        }

                        // 3) sale item row
                        using (MySqlCommand cmd = new MySqlCommand(PosSql.InsertSaleItem, _conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@sale", saleId);
                            cmd.Parameters.AddWithValue("@prod", it.ProductId);
                            cmd.Parameters.AddWithValue("@qty", it.Qty);
                            cmd.Parameters.AddWithValue("@price", it.Price);
                            cmd.Parameters.AddWithValue("@total", it.Total);
                            cmd.ExecuteNonQuery();
                        }

                        // 4) take the stock out (an OUT movement is how this database lowers stock)
                        using (MySqlCommand cmd = new MySqlCommand(PosSql.InsertMovement, _conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@prod", it.ProductId);
                            cmd.Parameters.AddWithValue("@qty", it.Qty);
                            cmd.Parameters.AddWithValue("@sale", saleId);
                            cmd.Parameters.AddWithValue("@cashier", cashier);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                    return saleId;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        // ---------------------------------------------------------
        // ORDER LOGIC
        // ---------------------------------------------------------
        private void AddProductFromRow(DataGridViewRow row)
        {
            try
            {
                string id = Convert.ToString(row.Cells["ProductId"].Value);
                string name = Convert.ToString(row.Cells["Product"].Value);
                decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
                int stock = Convert.ToInt32(row.Cells["Stocks"].Value);

                AddToOrder(id, name, price, stock);
            }
            catch
            {
                MessageBox.Show("Invalid product information.", "MIKMIK POS",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddToOrder(string id, string name, decimal price, int stock)
        {
            if (stock < 1)
            {
                MessageBox.Show(name + " is out of stock.", "MIKMIK POS",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (DataGridViewRow r in dataGridView2.Rows)
            {
                if (Convert.ToString(r.Cells["ProductId"].Value) == id)
                {
                    int qty = Convert.ToInt32(r.Cells["Qty"].Value) + 1;

                    if (qty > stock)
                    {
                        MessageBox.Show("Not enough stock for " + name +
                            ". Available: " + stock, "MIKMIK POS",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    r.Cells["Qty"].Value = qty;
                    r.Cells["Total"].Value = price * qty;
                    RecalculateTotals();
                    return;
                }
            }

            dataGridView2.Rows.Add(id, stock, name, price, 1, price);
            RecalculateTotals();
        }

        // Prices are VAT-INCLUSIVE.
        //   Regular sale : VATable sales = subtotal / 1.12,  VAT = subtotal - VATable sales
        //   Senior sale  : VAT-exempt, so the VAT is taken off first, then 20% off the price without VAT
        private void RecalculateTotals()
        {
            decimal sub = 0m;

            foreach (DataGridViewRow r in dataGridView2.Rows)
            {
                sub += Convert.ToDecimal(r.Cells["Total"].Value);
            }

            decimal withoutVat = Math.Round(sub / (1m + VatRate), 2);
            decimal vatPart = sub - withoutVat;

            _subtotal = sub;

            if (_seniorOn)
            {
                _vatable = 0m;
                _vat = 0m;
                _vatExempt = withoutVat;
                _vatRemoved = vatPart;
                _discount = Math.Round(withoutVat * 0.20m, 2);
                _total = withoutVat - _discount;
            }
            else
            {
                _vatable = withoutVat;
                _vat = vatPart;
                _vatExempt = 0m;
                _vatRemoved = 0m;
                _discount = 0m;
                _total = sub;
            }

            if (lblSubtotal != null) lblSubtotal.Text = "Subtotal: " + _subtotal.ToString("N2");
            if (lblDiscount != null) lblDiscount.Text = "Discount: " + _discount.ToString("N2");
            if (lblVat != null)
            {
                lblVat.Text = _seniorOn
                    ? "VAT 12%: EXEMPT"
                    : "VAT 12%: " + _vat.ToString("N2");
            }
            if (lblTotal != null) lblTotal.Text = "TOTAL: " + _total.ToString("N2");

            UpdateChange();
        }

        private bool TryGetCash(out decimal cash)
        {
            cash = 0m;

            if (txtCashTendered == null) return false;

            return decimal.TryParse(txtCashTendered.Text.Trim(), out cash);
        }

        private void UpdateChange()
        {
            if (lblChange == null) return;

            decimal cash;

            if (TryGetCash(out cash) && _total > 0 && cash >= _total)
            {
                lblChange.Text = "Change: " + (cash - _total).ToString("N2");
            }
            else
            {
                lblChange.Text = "Change: 0.00";
            }
        }

        private void FilterOrder()
        {
            string t = txtSearchOrder.Text.Trim().ToLower();
            dataGridView2.CurrentCell = null;

            foreach (DataGridViewRow r in dataGridView2.Rows)
            {
                r.Visible = t.Length == 0 ||
                    Convert.ToString(r.Cells["Product"].Value).ToLower().Contains(t);
            }
        }

        private void SetSenior(bool on)
        {
            _seniorOn = on;

            if (btnSeniorCitizen != null)
            {
                btnSeniorCitizen.Text = on ? "SENIOR: ON 20%\n[Alt+S]" : "SENIOR CITIZEN\n[Alt+S]";
                btnSeniorCitizen.BackColor = on ? Color.Orange : Color.SandyBrown;
            }

            RecalculateTotals();
        }

        private void ResetOrder()
        {
            dataGridView2.Rows.Clear();

            if (txtCashTendered != null) txtCashTendered.Clear();
            if (txtSearchOrder != null) txtSearchOrder.Clear();

            SetSenior(false);
        }

        // ---------------------------------------------------------
        // EVENTS
        // ---------------------------------------------------------
        private void TxtSearchProduct_KeyDown(object sender, KeyEventArgs e)
        {
            // Press Enter (barcode scanners send Enter) to add the product
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;

            string code = txtSearchProduct.Text.Trim();
            if (code.Length == 0) return;

            DataGridViewRow match = null;

            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                if (string.Equals(Convert.ToString(r.Cells["Barcode"].Value), code,
                    StringComparison.OrdinalIgnoreCase))
                {
                    match = r;
                    break;
                }
            }

            if (match == null && dataGridView1.Rows.Count == 1)
            {
                match = dataGridView1.Rows[0];
            }

            if (match == null && dataGridView1.CurrentRow != null)
            {
                match = dataGridView1.CurrentRow;
            }

            if (match != null)
            {
                AddProductFromRow(match);
                txtSearchProduct.Clear();
            }
        }

        private void TxtCashTendered_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void TxtCashTendered_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnPayAndPrint_Click(sender, EventArgs.Empty);
            }
        }

        private void DataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dataGridView2.Columns[e.ColumnIndex].Name != "Qty") return;

            DataGridViewRow r = dataGridView2.Rows[e.RowIndex];
            int stock = Convert.ToInt32(r.Cells["Stock"].Value);
            decimal price = Convert.ToDecimal(r.Cells["Price"].Value);
            int qty;

            if (!int.TryParse(Convert.ToString(r.Cells["Qty"].Value), out qty) || qty < 1)
            {
                MessageBox.Show("Quantity must be a whole number of 1 or more.",
                    "MIKMIK POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                qty = 1;
            }
            else if (qty > stock)
            {
                MessageBox.Show("Only " + stock + " in stock.", "MIKMIK POS",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                qty = stock;
            }

            r.Cells["Qty"].Value = qty;
            r.Cells["Total"].Value = price * qty;
            RecalculateTotals();
        }

        // ---------------------------------------------------------
        // BUTTONS
        // ---------------------------------------------------------
        private void BtnAddTo_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Please select a product.", "MIKMIK POS",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            AddProductFromRow(dataGridView1.CurrentRow);
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentRow == null)
            {
                MessageBox.Show("Please select an item in the current order.",
                    "MIKMIK POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            dataGridView2.Rows.Remove(dataGridView2.CurrentRow);
            RecalculateTotals();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (dataGridView2.Rows.Count > 0)
            {
                DialogResult ans = MessageBox.Show("Clear the whole order?", "MIKMIK POS",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (ans != DialogResult.Yes) return;
            }

            ResetOrder();
        }

        private void BtnSeniorCitizen_Click(object sender, EventArgs e)
        {
            SetSenior(!_seniorOn);
        }

        private void BtnPayAndPrint_Click(object sender, EventArgs e)
        {
            dataGridView2.EndEdit();

            if (_conn == null)
            {
                MessageBox.Show("No database connection.", "MIKMIK POS",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dataGridView2.Rows.Count == 0)
            {
                MessageBox.Show("The order is empty.", "MIKMIK POS",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal cash;

            if (!TryGetCash(out cash))
            {
                MessageBox.Show("Please enter a valid cash amount.", "MIKMIK POS",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (txtCashTendered != null) txtCashTendered.Focus();
                return;
            }

            if (cash < _total)
            {
                MessageBox.Show("Cash tendered is less than the total.", "MIKMIK POS",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCashTendered.Focus();
                return;
            }

            decimal change = cash - _total;

            List<OrderLine> items = new List<OrderLine>();

            foreach (DataGridViewRow r in dataGridView2.Rows)
            {
                items.Add(new OrderLine
                {
                    ProductId = Convert.ToString(r.Cells["ProductId"].Value),
                    Name = Convert.ToString(r.Cells["Product"].Value),
                    Price = Convert.ToDecimal(r.Cells["Price"].Value),
                    Qty = Convert.ToInt32(r.Cells["Qty"].Value),
                    Total = Convert.ToDecimal(r.Cells["Total"].Value)
                });
            }

            string saleId;

            try
            {
                saleId = SaveSale(items, cash, change);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The sale was NOT saved.\n\n" + ex.Message,
                    "MIKMIK POS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            PrintReceipt(saleId, items, cash, change);

            ResetOrder();
            LoadProducts(txtSearchProduct != null ? txtSearchProduct.Text.Trim() : "");
        }

        // ---------------------------------------------------------
        // SHORTCUT KEYS (letters, hold Alt)
        //   Alt+F = find / scan product      Alt+A = add selected product
        //   Alt+R = remove selected item     Alt+C = clear order
        //   Alt+S = senior citizen on/off    Alt+T = cash tendered
        //   Alt+Q = edit qty of selected     Alt+P = pay and print
        //   Up/Down (in search box) = move in product list
        //   Delete (in order list)  = remove selected item
        //   Esc = clear the search box
        // Alt is used so the letters never get typed into the search box.
        // ---------------------------------------------------------
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!_layoutReady) return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Alt | Keys.F:
                    if (txtSearchProduct != null)
                    {
                        txtSearchProduct.Focus();
                        txtSearchProduct.SelectAll();
                    }
                    return true;

                case Keys.Alt | Keys.A:
                    BtnAddTo_Click(this, EventArgs.Empty);
                    return true;

                case Keys.Alt | Keys.R:
                    BtnRemove_Click(this, EventArgs.Empty);
                    return true;

                case Keys.Alt | Keys.C:
                    BtnClear_Click(this, EventArgs.Empty);
                    return true;

                case Keys.Alt | Keys.S:
                    SetSenior(!_seniorOn);
                    return true;

                case Keys.Alt | Keys.T:
                    if (txtCashTendered != null)
                    {
                        txtCashTendered.Focus();
                        txtCashTendered.SelectAll();
                    }
                    return true;

                case Keys.Alt | Keys.Q:
                    if (dataGridView2.CurrentRow != null)
                    {
                        dataGridView2.Focus();
                        dataGridView2.CurrentCell = dataGridView2.CurrentRow.Cells["Qty"];
                        dataGridView2.BeginEdit(true);
                    }
                    return true;

                case Keys.Alt | Keys.P:
                    BtnPayAndPrint_Click(this, EventArgs.Empty);
                    return true;

                case Keys.Escape:
                    if (txtSearchProduct != null)
                    {
                        txtSearchProduct.Clear();
                        txtSearchProduct.Focus();
                    }
                    return true;

                case Keys.Down:
                case Keys.Up:
                    if (txtSearchProduct != null && txtSearchProduct.Focused)
                    {
                        MoveProductSelection(keyData == Keys.Down ? 1 : -1);
                        return true;
                    }
                    break;

                case Keys.Delete:
                    if (dataGridView2.Focused && !dataGridView2.IsCurrentCellInEditMode)
                    {
                        BtnRemove_Click(this, EventArgs.Empty);
                        return true;
                    }
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MoveProductSelection(int step)
        {
            if (dataGridView1.Rows.Count == 0) return;

            int index = dataGridView1.CurrentRow != null ? dataGridView1.CurrentRow.Index : -1;
            index = Math.Max(0, Math.Min(dataGridView1.Rows.Count - 1, index + step));

            dataGridView1.ClearSelection();
            dataGridView1.Rows[index].Selected = true;
            dataGridView1.CurrentCell = dataGridView1.Rows[index].Cells["Product"];
        }

        // ---------------------------------------------------------
        // RECEIPT
        // ---------------------------------------------------------
        private void PrintReceipt(string saleId, List<OrderLine> items, decimal cash, decimal change)
        {
            _receiptLines = BuildReceipt(saleId, items, cash, change);

            try
            {
                using (PrintDocument doc = new PrintDocument())
                {
                    doc.PrintPage += Doc_PrintPage;

                    using (PrintPreviewDialog preview = new PrintPreviewDialog())
                    {
                        preview.Document = doc;
                        preview.Width = 520;
                        preview.Height = 700;
                        preview.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The sale was saved, but the receipt could not be printed.\n\n" + ex.Message,
                    "MIKMIK POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private List<string> BuildReceipt(string saleId, List<OrderLine> items, decimal cash, decimal change)
        {
            const int W = 40;
            List<string> lines = new List<string>();

            lines.Add(CenterText("MIKMIK", W));
            lines.Add(CenterText("SALES RECEIPT", W));
            lines.Add("Receipt #: " + saleId);
            lines.Add("Date: " + DateTime.Now.ToString("MMM dd, yyyy hh:mm tt"));
            lines.Add(new string('-', W));

            foreach (OrderLine it in items)
            {
                lines.Add(it.Name.Length > W ? it.Name.Substring(0, W) : it.Name);
                lines.Add(TwoColumns("  " + it.Qty + " x " + it.Price.ToString("N2"),
                    it.Total.ToString("N2"), W));
            }

            lines.Add(new string('-', W));
            lines.Add(TwoColumns("Subtotal", _subtotal.ToString("N2"), W));

            if (_seniorOn)
            {
                lines.Add(TwoColumns("Less: VAT 12%", "-" + _vatRemoved.ToString("N2"), W));
                lines.Add(TwoColumns("Senior Citizen 20%", "-" + _discount.ToString("N2"), W));
            }

            lines.Add(TwoColumns("TOTAL", _total.ToString("N2"), W));
            lines.Add(TwoColumns("Cash", cash.ToString("N2"), W));
            lines.Add(TwoColumns("Change", change.ToString("N2"), W));
            lines.Add(new string('-', W));

            // VAT breakdown
            if (_seniorOn)
            {
                lines.Add(TwoColumns("VATable Sales", "0.00", W));
                lines.Add(TwoColumns("VAT-Exempt Sales", _vatExempt.ToString("N2"), W));
                lines.Add(TwoColumns("VAT (12%)", "0.00", W));
            }
            else
            {
                lines.Add(TwoColumns("VATable Sales", _vatable.ToString("N2"), W));
                lines.Add(TwoColumns("VAT-Exempt Sales", "0.00", W));
                lines.Add(TwoColumns("VAT (12%)", _vat.ToString("N2"), W));
            }

            lines.Add(new string('-', W));
            lines.Add(CenterText("Thank you! Come again.", W));

            return lines;
        }

        private static string CenterText(string s, int width)
        {
            if (s.Length >= width) return s;
            return new string(' ', (width - s.Length) / 2) + s;
        }

        private static string TwoColumns(string left, string right, int width)
        {
            int maxLeft = width - right.Length - 1;
            if (left.Length > maxLeft) left = left.Substring(0, maxLeft);
            return left.PadRight(width - right.Length) + right;
        }

        private void Doc_PrintPage(object sender, PrintPageEventArgs e)
        {
            using (Font font = new Font("Courier New", 9F))
            {
                float lineHeight = font.GetHeight(e.Graphics) + 2;
                float y = e.MarginBounds.Top;

                foreach (string line in _receiptLines)
                {
                    e.Graphics.DrawString(line, font, Brushes.Black, e.MarginBounds.Left, y);
                    y += lineHeight;
                }
            }

            e.HasMorePages = false;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }
    }

    // ===================== ALL SQL IN ONE PLACE =====================
    // Matches inventory_system.sql. Edit column names HERE only.
    internal static class PosSql
    {
        // price = selling_price; stock comes from vw_product_stock (IN minus OUT movements)
        public const string SelectProducts =
            "SELECT p.product_id, p.barcode, p.product_name, " +
            "       p.selling_price AS price, " +
            "       COALESCE(s.current_quantity, 0) AS stock " +
            "FROM tbl_product p " +
            "LEFT JOIN vw_product_stock s ON s.product_id = p.product_id " +
            "WHERE p.product_status = 'Active' " +
            "  AND (p.barcode LIKE @s OR p.product_name LIKE @s) " +
            "ORDER BY p.product_name";

        public const string FindCashier =
            "SELECT acc_id FROM tbl_account WHERE acc_name = @name LIMIT 1";

        // tax_amount now stores the 12% VAT that is contained in the total
        public const string InsertSale =
            "INSERT INTO tbl_sale (sale_id, cashier_id, subtotal, discount_amount, tax_amount, " +
            "total_amount, amount_paid, change_due, payment_method, sale_status, created_at) " +
            "VALUES (@id, @cashier, @subtotal, @discount, @tax, @total, @paid, @change, 'Cash', 'Completed', NOW())";

        public const string InsertSaleItem =
            "INSERT INTO tbl_sale_item (sale_id, product_id, quantity, unit_price, line_discount, line_total) " +
            "VALUES (@sale, @prod, @qty, @price, 0, @total)";

        public const string LockProduct =
            "SELECT product_id FROM tbl_product WHERE product_id = @prod FOR UPDATE";

        public const string StockOf =
            "SELECT COALESCE(SUM(CASE WHEN movement_type = 'IN' THEN quantity ELSE -quantity END), 0) " +
            "FROM tbl_stock_movement WHERE product_id = @prod";

        public const string InsertMovement =
            "INSERT INTO tbl_stock_movement " +
            "(product_id, movement_type, reason, quantity, reference_id, performed_by, remarks, created_at) " +
            "VALUES (@prod, 'OUT', 'Sale', @qty, @sale, @cashier, 'POS sale', NOW())";
    }
}