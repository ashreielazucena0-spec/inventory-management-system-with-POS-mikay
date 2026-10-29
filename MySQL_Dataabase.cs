using System;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace inventory_management_system_with_POS
{
    public class MySQL_Dataabase
    {
        public MySqlConnection conn = new MySqlConnection();
        public MySqlCommand cmd;
        public MySqlDataReader dr;


        private static bool _schemaChecked;

      
        private static string Setting(string key, string fallback)
        {
            string value = ConfigurationManager.AppSettings[key];

            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        public static string BuildConnectionString()
        {
            string password = Setting("DbPassword", "123456789");

            if (DataEncryptor.IsEncrypted(password))
            {
                password = DataEncryptor.Decrypt(password);
            }
            else
            {
                // Plain text found in app.config: protect it for next time.
                EncryptPasswordInConfig(password);
            }

            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.Server = Setting("DbServer", "localhost");
            builder.Database = Setting("DbName", "inventory_system");
            builder.UserID = Setting("DbUser", "root");
            builder.Password = password;

            return builder.ConnectionString;
        }

        // Replaces the plain DbPassword in the .exe.config with its encrypted
        // form. Does nothing if the key is not in the file (the built-in
        // default is never written). Any failure is ignored: the app still
        // connects using the plain value.
        private static void EncryptPasswordInConfig(string plainPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["DbPassword"]))
                {
                    return;
                }

                Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                KeyValueConfigurationElement item = cfg.AppSettings.Settings["DbPassword"];

                if (item == null)
                {
                    return;
                }

                item.Value = DataEncryptor.Encrypt(plainPassword);
                cfg.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception)
            {
                // No write permission or similar: keep working with the plain value.
            }
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
            catch (CryptographicException ex)
            {
                MessageBox.Show(
                    "The saved database password cannot be read on this PC / Windows user.\n\n" +
                    "Open the .exe.config file, set DbPassword to the plain password, and start the program again.\n\n" +
                    ex.Message,
                    "Database Password Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
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