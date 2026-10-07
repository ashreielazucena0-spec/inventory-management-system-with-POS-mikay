using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace inventory_management_system_with_POS {
    public class MySQL_Dataabase {
        public MySqlConnection conn = new MySqlConnection();
        public MySqlCommand cmd;
        public MySqlDataReader dr;

       
        private static bool _schemaChecked;

        // ------------------------------------------------------------
        // Optional app.config keys (inside <appSettings>):
        //   DbServer, DbName, DbUser, DbPassword
        // DbPassword may be plain text or an "enc:v1:..." value made
        // with the Security Tools screen. Missing keys fall back to the
        // original defaults so the app keeps working without changes.
        // ------------------------------------------------------------
        private static string Setting(string key, string fallback)
        {
            string value = ConfigurationManager.AppSettings[key];

            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string BuildConnectionString()
        {
            string password = Setting("DbPassword", "123456789");

            if (DataEncryptor.IsEncrypted(password))
            {
                password = DataEncryptor.Decrypt(password);
            }

            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.Server = Setting("DbServer", "localhost");
            builder.Database = Setting("DbName", "inventory_system");
            builder.UserID = Setting("DbUser", "root");
            builder.Password = password;

            return builder.ConnectionString;
        }

        public void ConnectToDataBase()
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.ConnectionString = BuildConnectionString();
                    conn.Open();
                }

                EnsurePasswordColumn();
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

        // A PBKDF2 hash is about 90 characters, so pass_word must be
        // at least VARCHAR(255). Widened once; if the database user has
        // no ALTER permission this is skipped quietly and
        // migrate_password_hash.sql can be run by hand instead.
        private void EnsurePasswordColumn()
        {
            if (_schemaChecked)
            {
                return;
            }

            _schemaChecked = true;

            try
            {
                string check = @"
                    SELECT CHARACTER_MAXIMUM_LENGTH
                    FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'tbl_account'
                      AND COLUMN_NAME = 'pass_word'";

                object length;

                using (MySqlCommand c = new MySqlCommand(check, conn))
                {
                    length = c.ExecuteScalar();
                }

                if (length == null || length == DBNull.Value)
                {
                    return;
                }

                if (Convert.ToInt64(length) < 255)
                {
                    using (MySqlCommand c = new MySqlCommand(
                        "ALTER TABLE tbl_account MODIFY COLUMN pass_word VARCHAR(255) NOT NULL", conn))
                    {
                        c.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                // No permission or table missing: ignore here. The SQL
                // script migrate_password_hash.sql does the same change.
            }
        }

        public void CloseConnection()
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }
    }
}
