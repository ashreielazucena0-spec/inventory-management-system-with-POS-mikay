using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace inventory_management_system_with_POS
{
    // Manage Stocks  ->  tbl_product + vw_product_stock + tbl_stock_movement
    //
    // The designer drew the input boxes as empty BUTTONS (button2..8, button15).
    // They are swapped for real TextBoxes in code (Swap), keeping the same position.
    //
    //   button15  search box          button5  barcode (was "Brand")
    //   button2   product id          button6  selling price
    //   button3   product name        button7  cost price (was "Reorder level")
    //   comboBox1 category            button8  total stock value = stock x cost price
    //   button4   current stock
    //
    // The schema has no brand and no reorder level, so those two fields now show
    // the barcode and the cost price. Low stock is flagged with
    // InventoryData.LowStockLevel instead.
    public partial class frmStocks : Form
    {
        private const string Peso = "\u20B1";

        private TextBox txtSearch;
        private TextBox txtProductId;
        private TextBox txtProductName;
        private TextBox txtStock;
        private TextBox txtBarcode;
        private TextBox txtPrice;
        private TextBox txtCost;
        private TextBox txtTotalValue;

        private bool _isNew;

        public frmStocks()
        {
            InitializeComponent();
        }

        private void frmStocks_Load(object sender, EventArgs e)
        {
            try
            {
                InventoryData.EnsureDefaults();

                BuildInputs();
                LoadCategories();
                ConfigureGrid();

                button1.Click += button1_Click;     // Search
                button9.Click += button9_Click;     // ADD
                button10.Click += button10_Click;   // SAVE
                button11.Click += button11_Click;   // DELETE
                button12.Click += button12_Click;   // UPDATE
                button13.Click += button13_Click;   // STOCK IN/OUT
                txtSearch.TextChanged += txtSearch_TextChanged;
                dataGridView1.CellClick += dataGridView1_CellClick;

                LoadStocks();
                StartNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------
        // Setup
        // ------------------------------------------------------------
        private TextBox Swap(Button placeholder)
        {
            TextBox box = new TextBox();
            box.Bounds = placeholder.Bounds;
            box.Anchor = placeholder.Anchor;
            box.Font = placeholder.Font;

            Control parent = placeholder.Parent;
            parent.Controls.Add(box);
            parent.Controls.Remove(placeholder);

            return box;
        }

        private void BuildInputs()
        {
            txtSearch = Swap(button15);
            txtProductId = Swap(button2);
            txtProductName = Swap(button3);
            txtStock = Swap(button4);
            txtBarcode = Swap(button5);
            txtPrice = Swap(button6);
            txtCost = Swap(button7);
            txtTotalValue = Swap(button8);

            txtProductId.ReadOnly = true;
            txtTotalValue.ReadOnly = true;

            label6.Text = "Barcode:";
            label7.Text = "Selling Price:";
            label8.Text = "Cost Price:";
            label9.Text = "Total Stock Value:";
            Column4.HeaderText = "Barcode";
            Column5.HeaderText = "Selling Price";
            Column7.HeaderText = "Cost Price";
            Column8.HeaderText = "Stock Value";
            button13.Text = "STOCK IN/OUT";

            txtStock.TextChanged += RecalculateValue;
            txtCost.TextChanged += RecalculateValue;
        }

        private void LoadCategories()
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.DataSource = InventoryData.GetCategories();
            comboBox1.DisplayMember = "category_name";
            comboBox1.ValueMember = "category_id";
            comboBox1.SelectedIndex = -1;
        }

        private void ConfigureGrid()
        {
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // ------------------------------------------------------------
        // Grid
        // ------------------------------------------------------------
        private void LoadStocks()
        {
            DataTable dt = InventoryData.GetProducts(txtSearch.Text);

            dataGridView1.Rows.Clear();

            foreach (DataRow r in dt.Rows)
            {
                int stock = Convert.ToInt32(r["stock"]);
                decimal cost = Convert.ToDecimal(r["cost_price"]);
                decimal price = Convert.ToDecimal(r["selling_price"]);

                // Grid order: ID, Name, Category, Barcode, Selling Price,
                //             Current Stock, Cost Price, Stock Value
                int index = dataGridView1.Rows.Add(
                    r["product_id"].ToString(),
                    r["product_name"].ToString(),
                    r["category_name"].ToString(),
                    r["barcode"] == DBNull.Value ? "" : r["barcode"].ToString(),
                    price.ToString("N2"),
                    stock.ToString(),
                    cost.ToString("N2"),
                    (stock * cost).ToString("N2"));

                DataGridViewRow row = dataGridView1.Rows[index];

                row.Tag = new string[]
                {
                    r["category_id"].ToString(),
                    cost.ToString("0.00", CultureInfo.InvariantCulture),
                    price.ToString("0.00", CultureInfo.InvariantCulture)
                };

                if (stock <= InventoryData.LowStockLevel)
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            Reload();
        }

        // Search button
        private void button1_Click(object sender, EventArgs e)
        {
            Reload();
        }

        private void Reload()
        {
            try
            {
                LoadStocks();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            string[] extra = row.Tag as string[];

            _isNew = false;
            txtStock.ReadOnly = true;
            label5.Text = "Current Stocks:";

            txtProductId.Text = Convert.ToString(row.Cells[0].Value);
            txtProductName.Text = Convert.ToString(row.Cells[1].Value);
            txtBarcode.Text = Convert.ToString(row.Cells[3].Value);
            txtStock.Text = Convert.ToString(row.Cells[5].Value);

            if (extra != null)
            {
                comboBox1.SelectedValue = extra[0];
                txtCost.Text = extra[1];
                txtPrice.Text = extra[2];
            }
        }

        private void RecalculateValue(object sender, EventArgs e)
        {
            int stock;
            decimal cost;

            if (int.TryParse(txtStock.Text.Trim(), out stock) &&
                decimal.TryParse(txtCost.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out cost))
            {
                txtTotalValue.Text = Peso + (stock * cost).ToString("N2");
            }
            else
            {
                txtTotalValue.Text = "";
            }
        }

        // ------------------------------------------------------------
        // ADD / SAVE / UPDATE / DELETE
        // ------------------------------------------------------------
        private void StartNew()
        {
            _isNew = true;

            txtProductId.Text = InventoryData.NextProductId();
            txtProductName.Clear();
            txtBarcode.Clear();
            txtPrice.Clear();
            txtCost.Clear();
            txtStock.Text = "0";
            txtStock.ReadOnly = false;
            label5.Text = "Opening Stocks:";
            comboBox1.SelectedIndex = -1;

            dataGridView1.ClearSelection();
            txtProductName.Focus();
        }

        // ADD
        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                StartNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool TryMoney(string text, out decimal value)
        {
            string clean = (text ?? "").Replace(Peso, "").Replace(",", "").Trim();

            if (clean.Length == 0)
            {
                value = 0;
                return false;
            }

            return decimal.TryParse(clean, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
                   && value >= 0;
        }

        private bool ReadForm(out decimal cost, out decimal price, out int stock)
        {
            cost = 0;
            price = 0;
            stock = 0;

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Please enter the product name.", "Missing information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductName.Focus();
                return false;
            }

            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Please choose a category.", "Missing information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox1.Focus();
                return false;
            }

            if (!TryMoney(txtPrice.Text, out price))
            {
                MessageBox.Show("Selling price must be a number (0 or more).", "Invalid price",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return false;
            }

            if (!TryMoney(txtCost.Text, out cost))
            {
                MessageBox.Show("Cost price must be a number (0 or more).", "Invalid price",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCost.Focus();
                return false;
            }

            if (_isNew)
            {
                if (!int.TryParse(txtStock.Text.Trim(), out stock) || stock < 0)
                {
                    MessageBox.Show("Opening stock must be a whole number (0 or more).", "Invalid stock",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtStock.Focus();
                    return false;
                }
            }

            return true;
        }

        // SAVE (new product)
        private void button10_Click(object sender, EventArgs e)
        {
            if (!_isNew)
            {
                MessageBox.Show("Click ADD first to enter a new product, or use UPDATE to change the selected one.",
                    "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal cost, price;
            int stock;

            if (!ReadForm(out cost, out price, out stock))
            {
                return;
            }

            try
            {
                InventoryData.InsertProduct(
                    txtProductId.Text.Trim(),
                    txtProductName.Text,
                    txtBarcode.Text,
                    Convert.ToString(comboBox1.SelectedValue),
                    cost, price, stock);

                LoadStocks();
                StartNew();

                MessageBox.Show("Product saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (MySqlException ex)
            {
                ShowSaveError(ex);
            }
        }

        // UPDATE
        private void button12_Click(object sender, EventArgs e)
        {
            if (_isNew || string.IsNullOrWhiteSpace(txtProductId.Text))
            {
                MessageBox.Show("Select a product in the list first.", "Update",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal cost, price;
            int stock;

            if (!ReadForm(out cost, out price, out stock))
            {
                return;
            }

            try
            {
                InventoryData.UpdateProduct(
                    txtProductId.Text.Trim(),
                    txtProductName.Text,
                    txtBarcode.Text,
                    Convert.ToString(comboBox1.SelectedValue),
                    cost, price);

                LoadStocks();
                StartNew();

                MessageBox.Show("Product updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (MySqlException ex)
            {
                ShowSaveError(ex);
            }
        }

        // DELETE
        private void button11_Click(object sender, EventArgs e)
        {
            if (_isNew || string.IsNullOrWhiteSpace(txtProductId.Text))
            {
                MessageBox.Show("Select a product in the list first.", "Delete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Delete \"" + txtProductName.Text + "\"?",
                "Confirm delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            try
            {
                bool onlyHidden = InventoryData.DeleteProduct(txtProductId.Text.Trim());

                LoadStocks();
                StartNew();

                MessageBox.Show(
                    onlyHidden
                        ? "This product already has sales or stock history, so it was set to Inactive and removed from the lists instead of being deleted."
                        : "Product deleted.",
                    "Delete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowSaveError(MySqlException ex)
        {
            if (ex.Number == 1062)
            {
                MessageBox.Show("That barcode (or product ID) is already used by another product.",
                    "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBarcode.Focus();
                return;
            }

            MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // ------------------------------------------------------------
        // STOCK IN/OUT and VIEW HISTORY
        // ------------------------------------------------------------
        private bool HasSelectedProduct()
        {
            if (_isNew || string.IsNullOrWhiteSpace(txtProductId.Text))
            {
                MessageBox.Show("Select a product in the list first.", "Stocks",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }

        // STOCK IN/OUT
        private void button13_Click(object sender, EventArgs e)
        {
            if (!HasSelectedProduct())
            {
                return;
            }

            int stock;
            decimal cost;

            int.TryParse(txtStock.Text.Trim(), out stock);
            decimal.TryParse(txtCost.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out cost);

            using (frmStockMovementDialog dlg = new frmStockMovementDialog(
                txtProductId.Text.Trim(), txtProductName.Text.Trim(), stock, cost))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    string id = txtProductId.Text;

                    Reload();
                    ReselectProduct(id);
                }
            }
        }

        // VIEW HISTORY (wired in the designer)
        private void button14_Click(object sender, EventArgs e)
        {
            if (!HasSelectedProduct())
            {
                return;
            }

            using (frmStockHistoryDialog dlg = new frmStockHistoryDialog(
                txtProductId.Text.Trim(), txtProductName.Text.Trim()))
            {
                dlg.ShowDialog(this);
            }
        }

        // After the list is reloaded, show the new stock figure of the same product.
        private void ReselectProduct(string productId)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToString(row.Cells[0].Value) == productId)
                {
                    row.Selected = true;
                    dataGridView1_CellClick(dataGridView1, new DataGridViewCellEventArgs(0, row.Index));
                    return;
                }
            }
        }

        // Handlers still referenced by the designer.
        private void button7_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }

        private void button15_Click(object sender, EventArgs e)
        {

        }
    }
}
