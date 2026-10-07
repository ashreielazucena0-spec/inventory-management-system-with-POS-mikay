using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace inventory_management_system_with_POS
{
    // Point of Sale  ->  tbl_sale, tbl_sale_item, tbl_stock_movement
    //
    // Control map (names come from the designer):
    //   textBox1   invoice no (next sale_id, read only)
    //   comboBox1  payment type (tbl_sale.payment_method)
    //   textBox2   barcode        - press Enter to add the product
    //   textBox7   product name   - press Enter to add (auto-complete from tbl_product)
    //   dataGridView1  cart: Qty | Product name | Price | Discount | Total
    //                  Qty and Discount can be edited. Delete key removes a line.
    //   textBox3   amount due (read only)      label8  net total
    //   textBox4   sale discount (tbl_sale.discount_amount)
    //   textBox5   cash received               textBox6  change (read only)
    //   button2 Cancel   button3 Save   button4 Print receipt
    public partial class FrmSales : Form
    {
        private const string Peso = "\u20B1";

        private Timer timer;
        private bool _busy;                       // stops cell-change events from calling themselves
        private string _lastReceipt = "";
        private readonly Dictionary<string, int> _stockById = new Dictionary<string, int>();

        public FrmSales()
        {
            InitializeComponent();
        }

        private void FrmSales_Load(object sender, EventArgs e)
        {
            // Clock
            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
            UpdateDateTime();

            // Payment types (tbl_sale.payment_method)
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Items.Clear();
            comboBox1.Items.Add("Cash");
            comboBox1.Items.Add("Check");
            comboBox1.Items.Add("Card");
            comboBox1.SelectedIndex = 0;

            label4.Text = "Amount due";
            textBox1.ReadOnly = true;
            textBox3.ReadOnly = true;
            textBox6.ReadOnly = true;

            // Cart grid: only Qty (0) and Discount (3) can be typed in.
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.Columns[0].ReadOnly = false;
            dataGridView1.Columns[1].ReadOnly = true;
            dataGridView1.Columns[2].ReadOnly = true;
            dataGridView1.Columns[3].ReadOnly = false;
            dataGridView1.Columns[4].ReadOnly = true;
            dataGridView1.Columns[2].DefaultCellStyle.Format = "N2";
            dataGridView1.Columns[3].DefaultCellStyle.Format = "N2";
            dataGridView1.Columns[4].DefaultCellStyle.Format = "N2";

            textBox2.KeyDown += textBox2_KeyDown;
            textBox7.KeyDown += textBox7_KeyDown;
            textBox4.TextChanged += AmountsChanged;
            textBox5.TextChanged += AmountsChanged;
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
            dataGridView1.KeyDown += dataGridView1_KeyDown;
            button2.Click += button2_Click;

            try
            {
                InventoryData.EnsureDefaults();

                AutoCompleteStringCollection names = new AutoCompleteStringCollection();
                names.AddRange(InventoryData.GetActiveProductNames().ToArray());

                textBox7.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                textBox7.AutoCompleteSource = AutoCompleteSource.CustomSource;
                textBox7.AutoCompleteCustomSource = names;

                textBox1.Text = InventoryData.PeekNextSaleId();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RecalculateTotals();
            textBox2.Focus();
        }

        // ------------------------------------------------------------
        // Clock
        // ------------------------------------------------------------
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            label9.Text = DateTime.Now.ToString("MMMM dd, yyyy");
            label10.Text = DateTime.Now.ToString("hh:mm:ss tt");
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }

            base.OnFormClosed(e);
        }

        // ------------------------------------------------------------
        // Adding products
        // ------------------------------------------------------------
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;

            string code = textBox2.Text.Trim();

            if (code.Length == 0)
            {
                return;
            }

            try
            {
                DataRow product = InventoryData.FindProductByCode(code);

                if (product == null)
                {
                    MessageBox.Show("No active product with barcode / ID \"" + code + "\".",
                        "Not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox2.SelectAll();
                    return;
                }

                AddToCart(product);
                textBox2.Clear();
                textBox2.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Existing handler name from the designer: the text itself needs no code,
        // the product is added when Enter is pressed.
        private void textBox7_TextChanged(object sender, EventArgs e) { }

        private void textBox7_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;

            string name = textBox7.Text.Trim();

            if (name.Length == 0)
            {
                return;
            }

            try
            {
                DataRow product = InventoryData.FindProductByName(name);

                if (product == null)
                {
                    MessageBox.Show("No active product named \"" + name + "\".",
                        "Not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox7.SelectAll();
                    return;
                }

                AddToCart(product);
                textBox7.Clear();
                textBox2.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddToCart(DataRow product)
        {
            string id = product["product_id"].ToString();
            string name = product["product_name"].ToString();
            decimal price = Convert.ToDecimal(product["selling_price"]);
            int stock = Convert.ToInt32(product["stock"]);

            _stockById[id] = stock;

            // Same product again: raise the quantity.
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToString(row.Tag) == id)
                {
                    int qty = ReadInt(row.Cells[0].Value, 1) + 1;

                    if (qty > stock)
                    {
                        MessageBox.Show("Only " + stock + " of \"" + name + "\" in stock.",
                            "Not enough stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    row.Cells[0].Value = qty;
                    RecalculateRow(row);
                    RecalculateTotals();
                    return;
                }
            }

            if (stock <= 0)
            {
                MessageBox.Show("\"" + name + "\" is out of stock.",
                    "Not enough stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _busy = true;

            int index = dataGridView1.Rows.Add(1, name, price, 0m, price);
            dataGridView1.Rows[index].Tag = id;

            _busy = false;

            RecalculateTotals();
        }

        // ------------------------------------------------------------
        // Cart editing
        // ------------------------------------------------------------
        private static int ReadInt(object value, int fallback)
        {
            int n;

            if (value != null && int.TryParse(Convert.ToString(value).Trim(), out n))
            {
                return n;
            }

            return fallback;
        }

        private static decimal ReadMoney(object value, decimal fallback)
        {
            decimal d;

            if (value is decimal)
            {
                return (decimal)value;
            }

            string text = Convert.ToString(value);

            if (text != null)
            {
                text = text.Replace(Peso, "").Replace(",", "").Trim();

                if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out d))
                {
                    return d;
                }
            }

            return fallback;
        }

        // Makes Qty / Discount valid and refreshes the line total.
        private void RecalculateRow(DataGridViewRow row)
        {
            string id = Convert.ToString(row.Tag);
            int stock;

            if (!_stockById.TryGetValue(id, out stock))
            {
                stock = int.MaxValue;
            }

            int qty = ReadInt(row.Cells[0].Value, 1);

            if (qty < 1)
            {
                qty = 1;
            }

            if (qty > stock)
            {
                MessageBox.Show("Only " + stock + " in stock.", "Not enough stock",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                qty = stock;
            }

            decimal price = ReadMoney(row.Cells[2].Value, 0m);
            decimal discount = ReadMoney(row.Cells[3].Value, 0m);

            if (discount < 0)
            {
                discount = 0;
            }

            if (discount > qty * price)
            {
                discount = qty * price;
            }

            _busy = true;
            row.Cells[0].Value = qty;
            row.Cells[3].Value = discount;
            row.Cells[4].Value = qty * price - discount;
            _busy = false;
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (_busy || e.RowIndex < 0)
            {
                return;
            }

            RecalculateRow(dataGridView1.Rows[e.RowIndex]);
            RecalculateTotals();
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete &&
                !dataGridView1.IsCurrentCellInEditMode &&
                dataGridView1.CurrentRow != null)
            {
                dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
                RecalculateTotals();
                e.Handled = true;
            }
        }

        // ------------------------------------------------------------
        // Totals
        // ------------------------------------------------------------
        private void AmountsChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RecalculateTotals();
        }

        private decimal GetSubtotal()
        {
            decimal subtotal = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                subtotal += ReadMoney(row.Cells[4].Value, 0m);
            }

            return subtotal;
        }

        private decimal GetSaleDiscount(decimal subtotal)
        {
            decimal discount = ReadMoney(textBox4.Text, 0m);

            if (discount < 0)
            {
                discount = 0;
            }

            if (discount > subtotal)
            {
                discount = subtotal;
            }

            return discount;
        }

        private bool IsCash()
        {
            return comboBox1.SelectedIndex <= 0;
        }

        private void RecalculateTotals()
        {
            decimal subtotal = GetSubtotal();
            decimal discount = GetSaleDiscount(subtotal);
            decimal tax = Math.Round((subtotal - discount) * InventoryData.TaxRate, 2);
            decimal total = subtotal - discount + tax;

            label8.Text = Peso + total.ToString("N2");
            textBox3.Text = total.ToString("N2");
            label3.Visible = dataGridView1.Rows.Count == 0;

            if (IsCash())
            {
                textBox5.Enabled = true;

                decimal received = ReadMoney(textBox5.Text, 0m);
                textBox6.Text = received >= total ? (received - total).ToString("N2") : "0.00";
            }
            else
            {
                // Card / check: exactly the amount due is paid.
                textBox5.Enabled = false;
                textBox6.Text = "0.00";
            }
        }

        // ------------------------------------------------------------
        // Cancel / Save / Print
        // ------------------------------------------------------------
        private void ClearSale()
        {
            dataGridView1.Rows.Clear();
            _stockById.Clear();
            textBox2.Clear();
            textBox7.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            comboBox1.SelectedIndex = 0;

            RecalculateTotals();
            textBox2.Focus();
        }

        // Cancel
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
            {
                DialogResult answer = MessageBox.Show(
                    "Cancel this sale and clear the cart?",
                    "Cancel sale",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (answer != DialogResult.Yes)
                {
                    return;
                }
            }

            ClearSale();
        }

        // Save
        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellInEditMode)
            {
                dataGridView1.EndEdit();
            }

            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Add at least one product first.", "Empty cart",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox2.Focus();
                return;
            }

            List<CartLine> lines = new List<CartLine>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                CartLine line = new CartLine();
                line.ProductId = Convert.ToString(row.Tag);
                line.ProductName = Convert.ToString(row.Cells[1].Value);
                line.Quantity = ReadInt(row.Cells[0].Value, 1);
                line.UnitPrice = ReadMoney(row.Cells[2].Value, 0m);
                line.LineDiscount = ReadMoney(row.Cells[3].Value, 0m);
                lines.Add(line);
            }

            decimal subtotal = 0;

            foreach (CartLine line in lines)
            {
                subtotal += line.LineTotal;
            }

            decimal discount = GetSaleDiscount(subtotal);
            decimal tax = Math.Round((subtotal - discount) * InventoryData.TaxRate, 2);
            decimal total = subtotal - discount + tax;

            decimal paid;
            decimal change;
            string method = Convert.ToString(comboBox1.SelectedItem);

            if (IsCash())
            {
                paid = ReadMoney(textBox5.Text, 0m);

                if (paid < total)
                {
                    MessageBox.Show("Cash received is less than the amount due (" + Peso + total.ToString("N2") + ").",
                        "Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox5.Focus();
                    return;
                }

                change = paid - total;
            }
            else
            {
                paid = total;
                change = 0;
            }

            try
            {
                string saleId = InventoryData.SaveSale(
                    lines, subtotal, discount, tax, total, paid, change, method);

                _lastReceipt = BuildReceipt(saleId, lines, subtotal, discount, tax, total, paid, change, method);

                MessageBox.Show(
                    "Sale " + saleId + " saved." + (change > 0 ? "\nChange: " + Peso + change.ToString("N2") : ""),
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                ClearSale();
                textBox1.Text = InventoryData.PeekNextSaleId();
            }
            catch (InvalidOperationException ex)
            {
                // e.g. someone else sold the last item in the meantime.
                MessageBox.Show(ex.Message, "Sale not saved", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Print receipt (of the last saved sale)
        private void button4_Click(object sender, EventArgs e)
        {
            if (_lastReceipt.Length == 0)
            {
                MessageBox.Show("Save a sale first, then print its receipt.", "Receipt",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (frmReceiptDialog dlg = new frmReceiptDialog(_lastReceipt))
            {
                dlg.ShowDialog(this);
            }
        }

        private string BuildReceipt(
            string saleId, List<CartLine> lines, decimal subtotal, decimal discount,
            decimal tax, decimal total, decimal paid, decimal change, string method)
        {
            const int width = 38;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Center(label13.Text.Trim(), width));
            sb.AppendLine(new string('-', width));
            sb.AppendLine("Receipt : " + saleId);
            sb.AppendLine("Date    : " + DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"));
            sb.AppendLine("Cashier : " + Session.AccountName);
            sb.AppendLine(new string('-', width));

            foreach (CartLine line in lines)
            {
                string name = line.ProductName.Length > width ? line.ProductName.Substring(0, width) : line.ProductName;
                sb.AppendLine(name);
                sb.AppendLine(Row(
                    "  " + line.Quantity + " x " + line.UnitPrice.ToString("N2"),
                    line.LineTotal.ToString("N2"), width));

                if (line.LineDiscount > 0)
                {
                    sb.AppendLine(Row("  discount", "-" + line.LineDiscount.ToString("N2"), width));
                }
            }

            sb.AppendLine(new string('-', width));
            sb.AppendLine(Row("Subtotal", subtotal.ToString("N2"), width));

            if (discount > 0)
            {
                sb.AppendLine(Row("Discount", "-" + discount.ToString("N2"), width));
            }

            if (tax > 0)
            {
                sb.AppendLine(Row("Tax", tax.ToString("N2"), width));
            }

            sb.AppendLine(Row("TOTAL", total.ToString("N2"), width));
            sb.AppendLine(Row(method, paid.ToString("N2"), width));
            sb.AppendLine(Row("Change", change.ToString("N2"), width));
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
        // Handlers still referenced by the designer (no action needed)
        // ------------------------------------------------------------
        private void label2_Click(object sender, EventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void timer1_Tick(object sender, EventArgs e) { }
    }
}
