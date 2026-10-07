using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace inventory_management_system_with_POS
{
    public partial class frmLoginform : Form
    {
        private readonly MySQL_Dataabase _db;

        public frmLoginform()
        {
            InitializeComponent();
            _db = new MySQL_Dataabase();
        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to exit?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.ExitThread();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
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

        private void frmLoginform_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            textUsername.Clear();
            textPassword.Clear();
            textUsername.Focus();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Frmcreateaccount_cs Main = new Frmcreateaccount_cs(_db);

            // The create-account form can be closed in several ways (X button,
            // exit icon, "back to login", success). Whatever the way, show this
            // login form again so the program never ends up with only hidden forms.
            Main.FormClosed += (s, args) =>
            {
                if (!this.IsDisposed)
                {
                    this.Show();
                    this.Activate();
                    textPassword.Clear();
                    textUsername.Focus();
                }
            };

            Main.Show();
            this.Hide();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string username = textUsername.Text.Trim();
            string password = textPassword.Text;

            if (username.Length == 0 || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            button3.Enabled = false;

            try
            {
                if (_db.conn.State != ConnectionState.Open)
                {
                    _db.ConnectToDataBase();
                }

                // ConnectToDataBase already showed the reason if it failed.
                if (_db.conn.State != ConnectionState.Open)
                {
                    return;
                }

                // Look the user up by username only; the password is checked in code.
                string stored = null;
                int accId = 0;
                string accName = "";
                string accType = "";

                using (MySqlCommand cmd = new MySqlCommand(
                    "SELECT acc_id, acc_name, acc_type, pass_word FROM tbl_account WHERE user_name = @username LIMIT 1", _db.conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            accId = Convert.ToInt32(reader["acc_id"]);
                            accName = Convert.ToString(reader["acc_name"]);
                            accType = Convert.ToString(reader["acc_type"]);
                            stored = Convert.ToString(reader["pass_word"]);
                        }
                    }
                }

                bool loginSuccess = false;
                string matchedPassword = password;

                if (stored == null)
                {
                    // Unknown user: spend the same time as a real check so
                    // response time does not reveal which usernames exist.
                    PasswordHasher.DummyVerify(password);
                }
                else if (PasswordHasher.IsHashed(stored))
                {
                    loginSuccess = PasswordHasher.Verify(password, stored);
                }
                else if (PasswordHasher.VerifyLegacyPlainText(password, stored))
                {
                    // Old account that still has a plain-text password.
                    loginSuccess = true;
                }
                else if (PasswordHasher.VerifyLegacyPlainText(password.Trim(), stored))
                {
                    // The old create-account screen saved passwords trimmed.
                    loginSuccess = true;
                    matchedPassword = password.Trim();
                }

                if (loginSuccess && PasswordHasher.NeedsRehash(stored))
                {
                    UpgradePasswordHash(username, stored, matchedPassword);
                }

                if (loginSuccess)
                {
                    Session.Start(accId, accName, accType);

                    MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Hide();

                    var frmMain = new frmMainform(_db);
                    frmMain.FormClosed += (s, args) =>
                    {
                        if (frmMain.LogoutRequested && !this.IsDisposed)
                        {
                            // Setting > Logout: back to the login screen.
                            textUsername.Clear();
                            textPassword.Clear();
                            this.Show();
                            this.Activate();
                            textUsername.Focus();
                        }
                        else
                        {
                            this.Close();
                        }
                    };
                    frmMain.Show();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textPassword.Clear();
                    textPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                button3.Enabled = true;
            }
        }

        // Replaces a plain-text (or weaker) stored password with a fresh hash.
        // "AND pass_word = @old" makes sure nothing is overwritten if the
        // value changed in the meantime. A failure here never blocks the login.
        private void UpgradePasswordHash(string username, string oldStored, string password)
        {
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(
                    "UPDATE tbl_account SET pass_word = @new WHERE user_name = @username AND pass_word = @old", _db.conn))
                {
                    cmd.Parameters.AddWithValue("@new", PasswordHasher.Hash(password));
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@old", oldStored);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                // Most likely pass_word is still too short (see migrate_password_hash.sql).
                // The user can still log in with the old value next time.
            }
        }
    }
}