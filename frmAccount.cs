using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;


namespace inventory_management_system_with_POS
{
    public partial class frmAccount : Form
    {
        private readonly MySQL_Dataabase _db;

        public frmAccount(MySQL_Dataabase db)
        {
            InitializeComponent();

            _db = db;

            textBox1.TextChanged += textBox1_TextChanged;

            dataGridView1.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dataGridView1.MultiSelect = false;
        }

        // ==========================================
        // FORM LOAD
        // ==========================================
        private void frmAccount_Load(object sender, EventArgs e)
        {
            LoadAccounts("");
        }

        // ==========================================
        // LOAD ACCOUNTS
        // ==========================================
        private void LoadAccounts(string srch)
        {
            try
            {
                if (_db.conn.State != ConnectionState.Open)
                {
                    _db.conn.Open();
                }

                string sql = @"
                    SELECT acc_id, acc_name, acc_type, user_name, created_at
                    FROM tbl_account
                    WHERE user_name LIKE @search
                    OR acc_name LIKE @search";

                using (MySqlCommand cmd =
                       new MySqlCommand(sql, _db.conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@search",
                        srch + "%"
                    );

                    using (MySqlDataAdapter da =
                           new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        dataGridView1.Rows.Clear();

                        int num = 0;

                        foreach (DataRow row in dt.Rows)
                        {
                            num++;

                            dataGridView1.Rows.Add(
                                num,
                                row["acc_id"].ToString(),
                                row["acc_name"].ToString(),
                                row["acc_type"].ToString(),
                                row["user_name"].ToString(),
                                "********",   // never display the stored password / hash
                                row["created_at"].ToString()
                            );
                        }
                    }
                }

                dataGridView1.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // ==========================================
        // SEARCH
        // ==========================================
        private void textBox1_TextChanged(
            object sender,
            EventArgs e)
        {
            LoadAccounts(textBox1.Text.Trim());
        }

        // ==========================================
        // EDIT / UPDATE BUTTON
        // ==========================================
        private void button2_Click(
            object sender,
            EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Please select an account to update first.",
                    "No Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            string accountId =
                dataGridView1.SelectedRows[0]
                .Cells[1]
                .Value
                .ToString();

            try
            {
                string accId = "";
                string accName = "";
                string accType = "";
                string username = "";

                if (_db.conn.State != ConnectionState.Open)
                {
                    _db.conn.Open();
                }

                // The password column is deliberately NOT selected.
                string sql = @"
                    SELECT acc_id,
                           acc_name,
                           acc_type,
                           user_name
                    FROM tbl_account
                    WHERE acc_id = @id";

                using (MySqlCommand cmd =
                       new MySqlCommand(sql, _db.conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@id",
                        accountId
                    );

                    using (MySqlDataReader dr =
                           cmd.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            MessageBox.Show(
                                "Account not found.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );

                            return;
                        }

                        accId =
                            dr["acc_id"].ToString();

                        accName =
                            dr["acc_name"].ToString();

                        accType =
                            dr["acc_type"].ToString();

                        username =
                            dr["user_name"].ToString();
                    }
                }

                // IMPORTANT:
                // DataReader is CLOSED here.

                using (Frmcreateaccount_cs createAccount =
                       new Frmcreateaccount_cs(_db))
                {
                    createAccount.EditMode = true;   // UPDATE instead of INSERT

                    createAccount.txtAccID.Text = accId;
                    createAccount.txtAccName.Text = accName;
                    createAccount.cbAccountType.Text = accType;
                    createAccount.textUsername.Text = username;
                    // Password box stays blank: blank = keep the current password.

                    createAccount.ShowDialog(this);
                }

                // Refresh table
                LoadAccounts(
                    textBox1.Text.Trim()
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading account:\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // ==========================================
        // DELETE BUTTON
        // ==========================================
        private void button1_Click(
            object sender,
            EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Please select an account to delete.",
                    "No Selection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            string accountId =
                dataGridView1.SelectedRows[0]
                .Cells[1]
                .Value
                .ToString();

            string accountName =
                dataGridView1.SelectedRows[0]
                .Cells[2]
                .Value
                .ToString();

            DialogResult confirm = MessageBox.Show(
                $"Are you sure you want to delete account '{accountName}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (_db.conn.State != ConnectionState.Open)
                {
                    _db.conn.Open();
                }

                string sql =
                    "DELETE FROM tbl_account WHERE acc_id = @id";

                using (MySqlCommand cmd =
                       new MySqlCommand(sql, _db.conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@id",
                        accountId
                    );

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show(
                    "Account deleted successfully.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                LoadAccounts(
                    textBox1.Text.Trim()
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // ==========================================
        // BACK BUTTON
        // ==========================================
        private void pictureBox2_Click(
            object sender,
            EventArgs e)
        {
            this.Close();
        }

        // ==========================================
        // DATAGRIDVIEW EVENT
        // ==========================================
        private void dataGridView1_CellContentClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
        }

        // ==========================================
        // EXTRA DESIGNER EVENT
        // ==========================================
        private void textBox1_TextChanged_1(
            object sender,
            EventArgs e)
        {
        }

        private void frmAccount_Load_1(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}