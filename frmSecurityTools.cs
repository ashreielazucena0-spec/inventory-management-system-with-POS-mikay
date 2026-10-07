using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace inventory_management_system_with_POS
{
    // ==========================================================
    // SECURITY TOOLS SCREEN
    //
    // Built completely in code (no designer file needed).
    // Opened from  Setting > Security Tools  in frmMainform.
    //
    //   Hash    : text            -> one-way hash   (for passwords)
    //   Verify  : text + hash     -> matches or not
    //   Encrypt : text            -> enc:v1:...     (can be decrypted)
    //   Decrypt : enc:v1:... text -> original text
    //
    // Typical use: encrypt the MySQL password here, then paste the
    // enc:v1:... result into app.config as the DbPassword value.
    // ==========================================================
    public class frmSecurityTools : Form
    {
        private readonly TextBox _txtInput;
        private readonly TextBox _txtValue;
        private readonly CheckBox _chkShow;
        private readonly Label _lblStatus;

        public frmSecurityTools()
        {
            Text = "Security Tools";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(560, 420);
            Font = new Font("Segoe UI", 9F);

            Label lblInput = new Label();
            lblInput.Text = "Text / password (to hash, verify or encrypt; decrypted text appears here):";
            lblInput.AutoSize = true;
            lblInput.Location = new Point(16, 16);

            _txtInput = new TextBox();
            _txtInput.Location = new Point(16, 38);
            _txtInput.Size = new Size(528, 25);
            _txtInput.UseSystemPasswordChar = true;

            _chkShow = new CheckBox();
            _chkShow.Text = "Show text";
            _chkShow.AutoSize = true;
            _chkShow.Location = new Point(16, 68);
            _chkShow.CheckedChanged += delegate
            {
                _txtInput.UseSystemPasswordChar = !_chkShow.Checked;
            };

            Label lblValue = new Label();
            lblValue.Text = "Hash or encrypted value (result of Hash / Encrypt; input for Verify / Decrypt):";
            lblValue.AutoSize = true;
            lblValue.Location = new Point(16, 100);

            _txtValue = new TextBox();
            _txtValue.Location = new Point(16, 122);
            _txtValue.Size = new Size(528, 120);
            _txtValue.Multiline = true;
            _txtValue.ScrollBars = ScrollBars.Vertical;
            _txtValue.Font = new Font("Consolas", 9F);

            Button btnHash = MakeButton("Hash", 16, 258);
            Button btnVerify = MakeButton("Verify", 150, 258);
            Button btnEncrypt = MakeButton("Encrypt", 284, 258);
            Button btnDecrypt = MakeButton("Decrypt", 418, 258);

            btnHash.Click += HashClicked;
            btnVerify.Click += VerifyClicked;
            btnEncrypt.Click += EncryptClicked;
            btnDecrypt.Click += DecryptClicked;

            _lblStatus = new Label();
            _lblStatus.AutoSize = false;
            _lblStatus.Location = new Point(16, 302);
            _lblStatus.Size = new Size(528, 56);
            _lblStatus.Text = "Hash is one-way (cannot be reversed). Encrypt can be decrypted again, "
                            + "but only by the same Windows user on this PC.";

            Button btnCopy = MakeButton("Copy value", 16, 370);
            btnCopy.Click += delegate
            {
                if (_txtValue.Text.Length > 0)
                {
                    Clipboard.SetText(_txtValue.Text);
                    SetStatus("Value copied to the clipboard.", false);
                }
            };

            Button btnClear = MakeButton("Clear", 284, 370);
            btnClear.Click += delegate
            {
                _txtInput.Clear();
                _txtValue.Clear();
                SetStatus(string.Empty, false);
                _txtInput.Focus();
            };

            Button btnClose = MakeButton("Close", 418, 370);
            btnClose.Click += delegate { Close(); };
            CancelButton = btnClose;

            Controls.AddRange(new Control[]
            {
                lblInput, _txtInput, _chkShow, lblValue, _txtValue,
                btnHash, btnVerify, btnEncrypt, btnDecrypt,
                _lblStatus, btnCopy, btnClear, btnClose
            });
        }

        private static Button MakeButton(string text, int x, int y)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(126, 30);
            return b;
        }

        private void SetStatus(string message, bool isError)
        {
            _lblStatus.ForeColor = isError ? Color.Firebrick : SystemColors.ControlText;
            _lblStatus.Text = message;
        }

        private void HashClicked(object sender, EventArgs e)
        {
            if (_txtInput.Text.Length == 0)
            {
                SetStatus("Type the text to hash first.", true);
                return;
            }

            Cursor = Cursors.WaitCursor;

            try
            {
                _txtValue.Text = PasswordHasher.Hash(_txtInput.Text);
                SetStatus("Hashed. A new random salt is used each time, so hashing the same text twice gives different results - that is normal.", false);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void VerifyClicked(object sender, EventArgs e)
        {
            string stored = _txtValue.Text.Trim();

            if (_txtInput.Text.Length == 0 || stored.Length == 0)
            {
                SetStatus("Fill in the text and the hash to compare.", true);
                return;
            }

            if (!PasswordHasher.IsHashed(stored))
            {
                SetStatus("The value is not a hash made by this program (it should start with PBKDF2-SHA256$).", true);
                return;
            }

            Cursor = Cursors.WaitCursor;

            try
            {
                if (PasswordHasher.Verify(_txtInput.Text, stored))
                {
                    SetStatus("MATCH - the text produces this hash.", false);
                }
                else
                {
                    SetStatus("NO MATCH - the text does not produce this hash.", true);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void EncryptClicked(object sender, EventArgs e)
        {
            if (_txtInput.Text.Length == 0)
            {
                SetStatus("Type the text to encrypt first.", true);
                return;
            }

            try
            {
                _txtValue.Text = DataEncryptor.Encrypt(_txtInput.Text);
                SetStatus("Encrypted. Only this Windows user on this PC can decrypt it.", false);
            }
            catch (Exception ex)
            {
                SetStatus("Could not encrypt: " + ex.Message, true);
            }
        }

        private void DecryptClicked(object sender, EventArgs e)
        {
            string value = _txtValue.Text.Trim();

            if (value.Length == 0)
            {
                SetStatus("Paste the encrypted value (enc:v1:...) first.", true);
                return;
            }

            try
            {
                _txtInput.Text = DataEncryptor.Decrypt(value);
                SetStatus("Decrypted. The original text is in the top box" +
                          (_chkShow.Checked ? "." : " (tick \"Show text\" to read it)."), false);
            }
            catch (CryptographicException ex)
            {
                SetStatus("Could not decrypt: " + ex.Message, true);
            }
            catch (Exception ex)
            {
                SetStatus("Could not decrypt: " + ex.Message, true);
            }
        }
    }
}
