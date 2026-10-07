using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace inventory_management_system_with_POS
{
    public partial class Frmcreateaccount_cs : Form
    {
        public readonly MySQL_Dataabase _db;

        public Frmcreateaccount_cs(MySQL_Dataabase db)
        {
            InitializeComponent();
            _db = db;
        }

        private bool _editMode;

        /// <summary>
        /// False = create a new account (INSERT).
        /// True  = edit the account whose ID is in txtAccID (UPDATE).
        ///         The ID cannot be changed, and a blank password keeps the current one.
        /// </summary>
        public bool EditMode
        {
            get { return _editMode; }
            set
            {
                _editMode = value;
                txtAccID.ReadOnly = value;
                button1.Text = value ? "Update" : "Save";

                ToolTip tip = new ToolTip();
                tip.SetToolTip(
                    textPassword,
                    value
                        ? "Leave blank to keep the current password."
                        : "At least " + PasswordHasher.MinimumLength + " characters.");
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void linkLabel1_LinkClicked(
            object sender,
            LinkLabelLinkClickedEventArgs e)
        {
        }

        private void textBox1_TextChanged(
            object sender,
            EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(
            object sender,
            EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textPassword.UseSystemPasswordChar = false;
                checkBox1.Text = "Hide Password";
            }
            else
            {
                textPassword.UseSystemPasswordChar = true;
                checkBox1.Text = "Show Password";
            }
        }

        private void comboBox1_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
        }

        private void panel1_Paint(
            object sender,
            PaintEventArgs e)
        {
        }

        // CLEAR BUTTON
        private void button2_Click(
            object sender,
            EventArgs e)
        {
            if (!_editMode)
            {
                txtAccID.Clear();
            }

            txtAccName.Clear();

            cbAccountType.SelectedIndex = -1;
            cbAccountType.Text = "";

            radioButton1.Checked = false;
            radioButton2.Checked = false;

            textUsername.Clear();
            textPassword.Clear();

            if (_editMode)
            {
                txtAccName.Focus();
            }
            else
            {
                txtAccID.Focus();
            }
        }

        // CREATE / UPDATE ACCOUNT BUTTON
        private void button1_Click_1(
            object sender,
            EventArgs e)
        {
            string accId = txtAccID.Text.Trim();
            string accName = txtAccName.Text.Trim();
            string accType = cbAccountType.Text.Trim();
            string username = textUsername.Text.Trim();
            string password = textPassword.Text;

            // CHECK REQUIRED FIELDS (password may stay blank when editing)
            if (accId.Length == 0 ||
                accName.Length == 0 ||
                accType.Length == 0 ||
                username.Length == 0 ||
                (!_editMode && string.IsNullOrWhiteSpace(password)))
            {
                MessageBox.Show(
                    "Please fill in all required fields.",
                    "Missing Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            bool changePassword = !string.IsNullOrEmpty(password);

            if (changePassword && password.Length < PasswordHasher.MinimumLength)
            {
                MessageBox.Show(
                    "The password must be at least " + PasswordHasher.MinimumLength + " characters long.",
                    "Weak Password",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                textPassword.Focus();
                return;
            }

            try
            {
                // OPEN DATABASE CONNECTION
                _db.ConnectToDataBase();

                if (_db.conn.State != ConnectionState.Open)
                {
                    return;
                }

                // DUPLICATE CHECKS
                if (!_editMode && Exists("SELECT COUNT(*) FROM tbl_account WHERE acc_id = @id", "@id", accId))
                {
                    MessageBox.Show(
                        "That account ID is already in use.",
                        "Duplicate Account ID",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    txtAccID.Focus();
                    return;
                }

                bool usernameTaken = _editMode
                    ? ExistsForOther(username, accId)
                    : Exists("SELECT COUNT(*) FROM tbl_account WHERE user_name = @id", "@id", username);

                if (usernameTaken)
                {
                    MessageBox.Show(
                        "That username is already taken. Please choose another one.",
                        "Duplicate Username",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    textUsername.Focus();
                    return;
                }

                string sql;

                if (_editMode)
                {
                    sql = changePassword
                        ? @"UPDATE tbl_account
                            SET acc_name = @acc_name,
                                acc_type = @acc_type,
                                user_name = @user_name,
                                pass_word = @pass_word
                            WHERE acc_id = @acc_id"
                        : @"UPDATE tbl_account
                            SET acc_name = @acc_name,
                                acc_type = @acc_type,
                                user_name = @user_name
                            WHERE acc_id = @acc_id";
                }
                else
                {
                    sql = @"
                        INSERT INTO tbl_account
                        (
                            acc_id,
                            acc_name,
                            acc_type,
                            user_name,
                            pass_word
                        )
                        VALUES
                        (
                            @acc_id,
                            @acc_name,
                            @acc_type,
                            @user_name,
                            @pass_word
                        )";
                }

                using (MySqlCommand cmd =
                       new MySqlCommand(sql, _db.conn))
                {
                    cmd.Parameters.AddWithValue("@acc_id", accId);
                    cmd.Parameters.AddWithValue("@acc_name", accName);
                    cmd.Parameters.AddWithValue("@acc_type", accType);
                    cmd.Parameters.AddWithValue("@user_name", username);

                    if (changePassword)
                    {
                        // Only the hash is stored, never the password itself.
                        cmd.Parameters.AddWithValue("@pass_word", PasswordHasher.Hash(password));
                    }

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show(
                    _editMode ? "Account updated successfully!" : "Account created successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                if (_editMode)
                {
                    this.DialogResult = DialogResult.OK;
                }

                // The login form (which opened this window) shows itself again
                // when this form closes, so there is no need to create a new one.
                this.Close();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Database Error:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                _db.CloseConnection();
            }
        }

        private bool Exists(string sql, string parameterName, string value)
        {
            using (MySqlCommand cmd = new MySqlCommand(sql, _db.conn))
            {
                cmd.Parameters.AddWithValue(parameterName, value);
                return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
            }
        }

        // Is this username used by an account other than the one being edited?
        private bool ExistsForOther(string username, string accId)
        {
            using (MySqlCommand cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM tbl_account WHERE user_name = @user AND acc_id <> @id", _db.conn))
            {
                cmd.Parameters.AddWithValue("@user", username);
                cmd.Parameters.AddWithValue("@id", accId);
                return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
            }
        }


        private void linkLabel1_LinkClicked_1(
            object sender,
            LinkLabelLinkClickedEventArgs e)
        {
            // The login form that opened this window shows itself again on close.
            this.Close();
        }



        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    "Are you sure you want to exit?",
                    "Exit Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
            {
                MessageBox.Show(
                    "Thank you for using the Inventory Management System with POS. Goodbye!",
                    "Exit Confirmation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.Close();
            }
        }

        private void textUsername_TextChanged(object sender, EventArgs e)
        {

        }

        private void b(object sender, EventArgs e)
        {

        }
    }
}