using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace inventory_management_system_with_POS
{
    // ==========================================================
    // Built completely in code (no designer files):
    //   frmSupplierManager  Management > Suppliers        -> tbl_supplier
    //   frmCategoryManager  Management > Categories       -> tbl_category
    //   frmLowStock         Report > Low Stock             -> vw_product_stock
    //   frmChangePassword   Setting > Change Password      -> tbl_account
    // ==========================================================

    // ---------------- shared look for the two editors ----------------
    public class frmEditorBase : Form
    {
        protected readonly DataGridView Grid;
        protected readonly TextBox TxtSearch;
        protected readonly Panel EditPanel;

        protected frmEditorBase(string title, int editHeight)
        {
            Text = title;
            BackColor = Color.FromArgb(174, 196, 222);
            Font = new Font("Segoe UI", 9F);
            ClientSize = new Size(1000, 620);

            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 52;
            header.BackColor = Color.DarkGray;
            Label t = new Label();
            t.Text = title;
            t.AutoSize = true;
            t.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            t.Location = new Point(30, 15);
            header.Controls.Add(t);

            Panel searchBar = new Panel();
            searchBar.Dock = DockStyle.Top;
            searchBar.Height = 42;
            Label l = new Label();
            l.Text = "Search:";
            l.SetBounds(22, 12, 55, 20);
            searchBar.Controls.Add(l);
            TxtSearch = new TextBox();
            TxtSearch.SetBounds(80, 9, 260, 25);
            TxtSearch.TextChanged += delegate { LoadList(); };
            searchBar.Controls.Add(TxtSearch);

            EditPanel = new Panel();
            EditPanel.Dock = DockStyle.Bottom;
            EditPanel.Height = editHeight;

            Grid = frmReportBase_Grid();
            Grid.Dock = DockStyle.Fill;
            Grid.SelectionChanged += delegate { OnRowSelected(); };

            Controls.Add(Grid);
            Controls.Add(EditPanel);
            Controls.Add(searchBar);
            Controls.Add(header);

            Load += delegate { LoadList(); };
        }

        private static DataGridView frmReportBase_Grid()
        {
            DataGridView g = new DataGridView();
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.MultiSelect = false;
            g.RowHeadersVisible = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.BackgroundColor = Color.White;
            return g;
        }

        protected TextBox AddField(string caption, int x, int y, int width, int max)
        {
            Label l = new Label();
            l.Text = caption;
            l.SetBounds(x, y + 3, 90, 20);
            EditPanel.Controls.Add(l);

            TextBox t = new TextBox();
            t.MaxLength = max;
            t.SetBounds(x + 92, y, width, 25);
            EditPanel.Controls.Add(t);
            return t;
        }

        protected Button AddAction(string text, int x, int y, EventHandler click)
        {
            Button b = new Button();
            b.Text = text;
            b.SetBounds(x, y, 110, 34);
            b.BackColor = SystemColors.ActiveCaption;
            b.UseVisualStyleBackColor = false;
            b.Click += click;
            EditPanel.Controls.Add(b);
            return b;
        }

        protected string SelectedId(string column)
        {
            DataRowView v = Grid.CurrentRow == null ? null : Grid.CurrentRow.DataBoundItem as DataRowView;
            return v == null ? null : Convert.ToString(v[column]);
        }

        protected DataRowView SelectedView()
        {
            return Grid.CurrentRow == null ? null : Grid.CurrentRow.DataBoundItem as DataRowView;
        }

        protected virtual void LoadList() { }
        protected virtual void OnRowSelected() { }

        protected static void Fail(Exception ex)
        {
            MySqlException my = ex as MySqlException;

            if (my != null && my.Number == 1062)
            {
                MessageBox.Show("That name already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (ex is InvalidOperationException)
            {
                MessageBox.Show(ex.Message, "Not allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ---------------- suppliers ----------------
    public class frmSupplierManager : frmEditorBase
    {
        private readonly TextBox _name, _contact, _phone, _email, _address;

        public frmSupplierManager() : base("SUPPLIERS", 150)
        {
            _name = AddField("Name:", 22, 14, 280, 150);
            _contact = AddField("Contact:", 22, 48, 280, 100);
            _phone = AddField("Phone:", 22, 82, 280, 20);
            _email = AddField("Email:", 440, 14, 300, 100);
            _address = AddField("Address:", 440, 48, 300, 255);

            AddAction("Add", 440, 90, btnAdd_Click);
            AddAction("Update", 560, 90, btnUpdate_Click);
            AddAction("Delete", 680, 90, btnDelete_Click);
            AddAction("Clear", 800, 90, delegate { ClearFields(); });
        }

        protected override void LoadList()
        {
            try
            {
                Grid.DataSource = InventoryData.GetSupplierList(TxtSearch.Text);
                SetHeader("supplier_id", "ID");
                SetHeader("supplier_name", "Supplier");
                SetHeader("contact_person", "Contact");
                SetHeader("phone", "Phone");
                SetHeader("email", "Email");
                SetHeader("address", "Address");
            }
            catch (Exception ex) { Fail(ex); }
        }

        private void SetHeader(string col, string text)
        {
            if (Grid.Columns.Contains(col)) { Grid.Columns[col].HeaderText = text; }
        }

        protected override void OnRowSelected()
        {
            DataRowView v = SelectedView();
            if (v == null) { return; }
            _name.Text = Convert.ToString(v["supplier_name"]);
            _contact.Text = Convert.ToString(v["contact_person"]);
            _phone.Text = Convert.ToString(v["phone"]);
            _email.Text = Convert.ToString(v["email"]);
            _address.Text = Convert.ToString(v["address"]);
        }

        private void ClearFields()
        {
            _name.Clear(); _contact.Clear(); _phone.Clear(); _email.Clear(); _address.Clear();
            Grid.ClearSelection();
            _name.Focus();
        }

        private bool Valid()
        {
            if (_name.Text.Trim().Length == 0)
            {
                MessageBox.Show("Supplier name is required.", "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (_email.Text.Trim().Length > 0 && _email.Text.IndexOf('@') < 1)
            {
                MessageBox.Show("Email does not look right.", "Supplier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!Valid()) { return; }
            try
            {
                InventoryData.InsertSupplier(_name.Text, _contact.Text, _phone.Text, _email.Text, _address.Text);
                ClearFields();
                LoadList();
            }
            catch (Exception ex) { Fail(ex); }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string id = SelectedId("supplier_id");
            if (id == null) { MessageBox.Show("Select a supplier first.", "Supplier"); return; }
            if (!Valid()) { return; }
            try
            {
                InventoryData.UpdateSupplier(id, _name.Text, _contact.Text, _phone.Text, _email.Text, _address.Text);
                LoadList();
            }
            catch (Exception ex) { Fail(ex); }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string id = SelectedId("supplier_id");
            if (id == null) { MessageBox.Show("Select a supplier first.", "Supplier"); return; }

            if (MessageBox.Show("Delete supplier \"" + _name.Text + "\"?\nProducts keep working; their supplier is cleared.",
                    "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                InventoryData.DeleteSupplier(id);
                ClearFields();
                LoadList();
            }
            catch (Exception ex) { Fail(ex); }
        }
    }

    // ---------------- categories ----------------
    public class frmCategoryManager : frmEditorBase
    {
        private readonly TextBox _name;

        public frmCategoryManager() : base("CATEGORIES", 90)
        {
            _name = AddField("Name:", 22, 14, 280, 100);

            AddAction("Add", 22, 46, btnAdd_Click);
            AddAction("Update", 142, 46, btnUpdate_Click);
            AddAction("Delete", 262, 46, btnDelete_Click);
            AddAction("Clear", 382, 46, delegate { _name.Clear(); Grid.ClearSelection(); _name.Focus(); });
        }

        protected override void LoadList()
        {
            try
            {
                DataTable all = InventoryData.GetCategoryList();
                string s = TxtSearch.Text.Trim();

                if (s.Length > 0)
                {
                    DataView dv = all.DefaultView;
                    dv.RowFilter = "category_name LIKE '%" + s.Replace("'", "''").Replace("[", "[[]") + "%'";
                    Grid.DataSource = dv;
                }
                else
                {
                    Grid.DataSource = all;
                }

                if (Grid.Columns.Contains("category_id")) { Grid.Columns["category_id"].HeaderText = "ID"; }
                if (Grid.Columns.Contains("category_name")) { Grid.Columns["category_name"].HeaderText = "Category"; }
                if (Grid.Columns.Contains("products")) { Grid.Columns["products"].HeaderText = "Products"; }
            }
            catch (Exception ex) { Fail(ex); }
        }

        protected override void OnRowSelected()
        {
            DataRowView v = SelectedView();
            if (v != null) { _name.Text = Convert.ToString(v["category_name"]); }
        }

        private bool Valid()
        {
            if (_name.Text.Trim().Length == 0)
            {
                MessageBox.Show("Category name is required.", "Category", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!Valid()) { return; }
            try { InventoryData.InsertCategory(_name.Text); _name.Clear(); LoadList(); }
            catch (Exception ex) { Fail(ex); }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string id = SelectedId("category_id");
            if (id == null) { MessageBox.Show("Select a category first.", "Category"); return; }
            if (!Valid()) { return; }
            try { InventoryData.UpdateCategory(id, _name.Text); LoadList(); }
            catch (Exception ex) { Fail(ex); }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string id = SelectedId("category_id");
            if (id == null) { MessageBox.Show("Select a category first.", "Category"); return; }

            if (MessageBox.Show("Delete category \"" + _name.Text + "\"?", "Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try { InventoryData.DeleteCategory(id); _name.Clear(); LoadList(); }
            catch (Exception ex) { Fail(ex); }
        }
    }

    // ---------------- low stock ----------------
    public class frmLowStock : frmReportBase
    {
        private readonly NumericUpDown _level;

        public frmLowStock() : base("LOW STOCK", "low_stock", false)
        {
            _level = new NumericUpDown();
            _level.Minimum = 0;
            _level.Maximum = 100000;
            _level.Value = InventoryData.LowStockLevel;
            _level.Width = 70;
            _level.ValueChanged += delegate { if (IsHandleCreated) { Reload(); } };
            AddFilter("Stock at or below:", _level);

            AddButton("Restock selected", btnRestock_Click);

            Grid.DataBindingComplete += delegate
            {
                foreach (DataGridViewRow row in Grid.Rows)
                {
                    DataRowView v = row.DataBoundItem as DataRowView;
                    if (v != null && Int(v["stock"]) <= 0)
                    {
                        row.DefaultCellStyle.BackColor = Color.MistyRose;
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                }
            };
        }

        protected override void LoadData()
        {
            DataTable dt = InventoryData.GetLowStock((int)_level.Value, TxtSearch.Text);
            Grid.DataSource = dt;

            Format(Grid, "product_id", "Product ID", null, false);
            Format(Grid, "product_name", "Product", null, false);
            Format(Grid, "category_name", "Category", null, false);
            Format(Grid, "supplier_name", "Supplier", null, false);
            Format(Grid, "stock", "In stock", "N0", true);
            Format(Grid, "cost_price", "Cost", "N2", true);

            int outOf = 0;
            foreach (DataRow r in dt.Rows) { if (Int(r["stock"]) <= 0) { outOf++; } }

            SummaryLabel.Text = "Products: " + dt.Rows.Count + "     Out of stock: " + outOf;
        }

        private void btnRestock_Click(object sender, EventArgs e)
        {
            DataRow r = SelectedRow();

            if (r == null)
            {
                MessageBox.Show("Select a product first.", "Restock", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (frmStockMovementDialog dlg = new frmStockMovementDialog(
                Convert.ToString(r["product_id"]), Convert.ToString(r["product_name"]),
                Int(r["stock"]), Dec(r["cost_price"]), 0))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK) { Reload(); }
            }
        }
    }

    // ---------------- change own password ----------------
    public class frmChangePassword : Form
    {
        private readonly TextBox _old, _new, _confirm;

        public frmChangePassword()
        {
            Text = "Change Password";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(400, 220);
            Font = new Font("Segoe UI", 9F);

            _old = Row("Current password:", 20);
            _new = Row("New password:", 60);
            _confirm = Row("Repeat new:", 100);

            Label hint = new Label();
            hint.Text = "At least 8 characters.";
            hint.SetBounds(150, 128, 230, 20);
            Controls.Add(hint);

            Button ok = new Button();
            ok.Text = "Save";
            ok.SetBounds(190, 165, 90, 32);
            ok.Click += btnSave_Click;
            Controls.Add(ok);

            Button cancel = new Button();
            cancel.Text = "Cancel";
            cancel.SetBounds(290, 165, 90, 32);
            cancel.DialogResult = DialogResult.Cancel;
            Controls.Add(cancel);

            AcceptButton = ok;
            CancelButton = cancel;
        }

        private TextBox Row(string caption, int y)
        {
            Label l = new Label();
            l.Text = caption;
            l.SetBounds(20, y + 3, 125, 20);
            Controls.Add(l);

            TextBox t = new TextBox();
            t.UseSystemPasswordChar = true;
            t.SetBounds(150, y, 230, 25);
            Controls.Add(t);
            return t;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            int? id = Session.AccountId;

            if (!id.HasValue)
            {
                MessageBox.Show("Nobody is logged in.", "Password");
                return;
            }

            if (_new.Text.Length < 8)
            {
                MessageBox.Show("The new password must have at least 8 characters.", "Password",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_new.Text != _confirm.Text)
            {
                MessageBox.Show("The two new passwords are not the same.", "Password",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string stored = InventoryData.GetPasswordValue(id.Value);
                bool ok;

                if (stored == null) { ok = false; }
                else if (PasswordHasher.IsHashed(stored)) { ok = PasswordHasher.Verify(_old.Text, stored); }
                else { ok = PasswordHasher.VerifyLegacyPlainText(_old.Text, stored); }

                if (!ok)
                {
                    MessageBox.Show("The current password is wrong.", "Password",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _old.Clear();
                    _old.Focus();
                    return;
                }

                InventoryData.SetPasswordValue(id.Value, PasswordHasher.Hash(_new.Text));

                MessageBox.Show("Password changed.", "Password", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
