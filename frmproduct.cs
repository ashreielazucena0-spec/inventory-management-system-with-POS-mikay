using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace inventory_management_system_with_POS
{
    public partial class frmproduct : Form
    {
        private const string Peso = "\u20B1";
        private const int ExpiryColumnIndex = 6;   // ID, NAME, CATEGORY, BARCODE, PRICE, STOCKS, EXPIRES

        private TextBox txtCost;
        private bool _isNew;

        public frmproduct()
        {
            InitializeComponent();
        }

        private void frmproduct_Load(object sender, EventArgs e)
        {
            try
            {
                InventoryData.EnsureDefaults();

                BuildExtras();
                LoadCategories();
                ConfigureGrid();

                textBox1.TextChanged += textBox1_TextChanged;
                dataGridView1.CellClick += dataGridView1_CellClick;
                button4.Click += button4_Click;
                button5.Click += button5_Click;
                button6.Click += button6_Click;

                LoadProducts();
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
        private void BuildExtras()
        {
            // The schema has a barcode, not a brand.
            labelBrand.Text = "Barcode:";
            Column4.HeaderText = "BARCODE";
            labelPrice.Text = "Selling price:";
            labelStock.Text = "Opening stock:";

            maskedTextBox1.ReadOnly = true;

            // Expiry date picker: unchecked means "no expiry".
            label2.Text = "Expiry date:";
            dateTimePicker1.Format = DateTimePickerFormat.Short;
            dateTimePicker1.ShowCheckBox = true;
            dateTimePicker1.Checked = false;

            Label lblCost = new Label();
            lblCost.Text = "Cost price:";
            lblCost.AutoSize = true;
            lblCost.Font = labelPrice.Font;
            lblCost.Location = new Point(labelPrice.Left, labelPrice.Top + 44);

            txtCost = new TextBox();
            txtCost.Location = new Point(textBox2.Left, textBox2.Top + 44);
            txtCost.Size = textBox2.Size;

            Controls.Add(lblCost);
            Controls.Add(txtCost);
            lblCost.BringToFront();
            txtCost.BringToFront();

            // Make room for the extra row.
            dataGridView1.Top += 44;
            dataGridView1.Height -= 44;
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

            // Extra column for the expiry date.
            if (!dataGridView1.Columns.Contains("colExpiry"))
            {
                dataGridView1.Columns.Add("colExpiry", "EXPIRES");
            }
        }

        // ------------------------------------------------------------
        // Grid
        // ------------------------------------------------------------
        private void LoadProducts()
        {
            DataTable dt = InventoryData.GetProducts(textBox1.Text);

            dataGridView1.Rows.Clear();

            foreach (DataRow r in dt.Rows)
            {
                string expiryText = "";
                DateTime expiryDate = DateTime.MinValue;
                bool hasExpiry = dt.Columns.Contains("expiry_date")
                                 && r["expiry_date"] != DBNull.Value;

                if (hasExpiry)
                {
                    expiryDate = Convert.ToDateTime(r["expiry_date"]);
                    expiryText = expiryDate.ToString("yyyy-MM-dd");
                }

                // Grid order: ID, NAME, CATEGORY, BARCODE, PRICE, STOCKS, EXPIRES
                int index = dataGridView1.Rows.Add(
                    r["product_id"].ToString(),
                    r["product_name"].ToString(),
                    r["category_name"].ToString(),
                    r["barcode"] == DBNull.Value ? "" : r["barcode"].ToString(),
                    Convert.ToDecimal(r["selling_price"]).ToString("N2"),
                    Convert.ToInt32(r["stock"]).ToString(),
                    expiryText);

                // Keep the numbers the form needs for editing.
                dataGridView1.Rows[index].Tag = new string[]
                {
                    r["category_id"].ToString(),
                    Convert.ToDecimal(r["cost_price"]).ToString("0.00", CultureInfo.InvariantCulture),
                    Convert.ToDecimal(r["selling_price"]).ToString("0.00", CultureInfo.InvariantCulture)
                };

                // Red = already expired, orange = expires within 30 days.
                if (hasExpiry)
                {
                    if (expiryDate.Date < DateTime.Today)
                    {
                        dataGridView1.Rows[index].DefaultCellStyle.BackColor = Color.LightCoral;
                    }
                    else if (expiryDate.Date <= DateTime.Today.AddDays(30))
                    {
                        dataGridView1.Rows[index].DefaultCellStyle.BackColor = Color.Orange;
                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                LoadProducts();
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
            maskedTextBox6.ReadOnly = true;
            labelStock.Text = "Current stock:";

            maskedTextBox1.Text = Convert.ToString(row.Cells[0].Value);
            maskedTextBox2.Text = Convert.ToString(row.Cells[1].Value);
            maskedTextBox4.Text = Convert.ToString(row.Cells[3].Value);
            maskedTextBox6.Text = Convert.ToString(row.Cells[5].Value);

            // Expiry date
            string exp = Convert.ToString(row.Cells[ExpiryColumnIndex].Value);
            DateTime parsed;
            if (DateTime.TryParse(exp, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                dateTimePicker1.Value = parsed;
                dateTimePicker1.Checked = true;
            }
            else
            {
                dateTimePicker1.Value = DateTime.Today;
                dateTimePicker1.Checked = false;
            }

            if (extra != null)
            {
                comboBox1.SelectedValue = extra[0];
                txtCost.Text = extra[1];
                textBox2.Text = extra[2];
            }
        }

        // ------------------------------------------------------------
        // New / Save / Update / Delete / Exit
        // ------------------------------------------------------------
        private void StartNew()
        {
            _isNew = true;

            maskedTextBox1.Text = InventoryData.NextProductId();
            maskedTextBox2.Clear();
            maskedTextBox4.Clear();
            textBox2.Clear();
            txtCost.Clear();
            maskedTextBox6.Text = "0";
            maskedTextBox6.ReadOnly = false;
            labelStock.Text = "Opening stock:";
            comboBox1.SelectedIndex = -1;

            dateTimePicker1.Value = DateTime.Today;
            dateTimePicker1.Checked = false;

            dataGridView1.ClearSelection();
            maskedTextBox2.Focus();
        }

        // Add
        private void button2_Click(object sender, EventArgs e)
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

        // Returns the chosen expiry date, or null when the picker is unchecked.
        private DateTime? GetExpiry()
        {
            return dateTimePicker1.Checked ? dateTimePicker1.Value.Date : (DateTime?)null;
        }

        private bool ReadForm(out decimal cost, out decimal price, out int stock)
        {
            cost = 0;
            price = 0;
            stock = 0;

            if (string.IsNullOrWhiteSpace(maskedTextBox2.Text))
            {
                MessageBox.Show("Please enter the product name.", "Missing information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                maskedTextBox2.Focus();
                return false;
            }

            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Please choose a category.", "Missing information",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox1.Focus();
                return false;
            }

            if (!TryMoney(textBox2.Text, out price))
            {
                MessageBox.Show("Selling price must be a number (0 or more).", "Invalid price",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox2.Focus();
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
                if (!int.TryParse(maskedTextBox6.Text.Trim(), out stock) || stock < 0)
                {
                    MessageBox.Show("Opening stock must be a whole number (0 or more).", "Invalid stock",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    maskedTextBox6.Focus();
                    return false;
                }
            }

            // A new product should not already be expired.
            if (_isNew && dateTimePicker1.Checked && dateTimePicker1.Value.Date < DateTime.Today)
            {
                MessageBox.Show("Expiry date cannot be in the past.", "Invalid date",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dateTimePicker1.Focus();
                return false;
            }

            return true;
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

        // Save (new product)
        private void button3_Click(object sender, EventArgs e)
        {
            if (!_isNew)
            {
                MessageBox.Show(
                    "Click Add first to enter a new product, or use Update to change the selected one.",
                    "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal cost, price;
            int stock;

            if (!ReadForm(out cost, out price, out stock))
                return;

            try
            {
                InventoryData.InsertProduct(
                    maskedTextBox1.Text.Trim(),
                    maskedTextBox2.Text.Trim(),
                    maskedTextBox4.Text.Trim(),
                    Convert.ToString(comboBox1.SelectedValue),
                    cost,
                    price,
                    stock,
                    GetExpiry()                 // saves the expiry date
                );

                LoadProducts();
                StartNew();

                MessageBox.Show("Product saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (MySqlException ex)
            {
                ShowSaveError(ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Update
        private void button4_Click(object sender, EventArgs e)
        {
            if (_isNew || string.IsNullOrWhiteSpace(maskedTextBox1.Text))
            {
                MessageBox.Show("Please select a product first.", "Update",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal cost, price;
            int stock;

            if (!ReadForm(out cost, out price, out stock))
                return;

            try
            {
                InventoryData.UpdateProduct(
                    maskedTextBox1.Text.Trim(),
                    maskedTextBox2.Text.Trim(),
                    maskedTextBox4.Text.Trim(),
                    Convert.ToString(comboBox1.SelectedValue),
                    cost,
                    price,
                    GetExpiry()                 // saves the expiry date
                );

                LoadProducts();
                StartNew();

                MessageBox.Show("Product updated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (MySqlException ex)
            {
                ShowSaveError(ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Delete
        private void button5_Click(object sender, EventArgs e)
        {
            if (_isNew || string.IsNullOrWhiteSpace(maskedTextBox1.Text))
            {
                MessageBox.Show("Select a product in the list first.", "Delete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Delete \"" + maskedTextBox2.Text + "\"?",
                "Confirm delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            try
            {
                bool onlyHidden = InventoryData.DeleteProduct(maskedTextBox1.Text.Trim());

                LoadProducts();
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

        // Exit
        private void button6_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ShowSaveError(MySqlException ex)
        {
            // 1062 = duplicate key. The only unique columns here are the id and the barcode.
            if (ex.Number == 1062)
            {
                MessageBox.Show("That barcode (or product ID) is already used by another product.",
                    "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                maskedTextBox4.Focus();
                return;
            }

            MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Handlers still referenced by the designer.
        private void label1_Click(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void labelCategory_Click(object sender, EventArgs e) { }
    }
}