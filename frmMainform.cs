using System;
using System.Windows.Forms;

namespace inventory_management_system_with_POS
{
    public partial class frmMainform : Form
    {
        private readonly MySQL_Dataabase _db;
        private Form _currentChild;

        public frmMainform() : this(null)
        {
        }

        public frmMainform(MySQL_Dataabase db)
        {
            InitializeComponent();
            _db = db;
            AddSecurityToolsMenu();
            AddReportMenus();
            logoutToolStripMenuItem.Click += logoutToolStripMenuItem_Click;
        }

        // True when the user chose Setting > Logout. frmLoginform reads this
        // to show the login screen again instead of ending the program.
        public bool LogoutRequested { get; private set; }

        // Adds the Report drop-down items in code, so the designer file is not touched.
        //   Sales Report  : Manager (and Admin)
        //   Expired Items : Inventory Staff, Manager
        //   Damage Report : Inventory Staff, Manager
        private void AddReportMenus()
        {
            ToolStripMenuItem sales = new ToolStripMenuItem("Sales Report");
            sales.Name = "salesReportToolStripMenuItem";
            sales.Click += delegate
            {
                if (Allow(Session.Manager))
                {
                    OpenChildForm(new frmManageReport());
                }
            };

            ToolStripMenuItem expired = new ToolStripMenuItem("Expired Items");
            expired.Name = "expiredItemsToolStripMenuItem";
            expired.Click += delegate
            {
                if (Allow(Session.InventoryStaff, Session.Manager))
                {
                    OpenChildForm(new frmExpireditem());
                }
            };

            ToolStripMenuItem damage = new ToolStripMenuItem("Damage Report");
            damage.Name = "damageReportToolStripMenuItem";
            damage.Click += delegate
            {
                if (Allow(Session.InventoryStaff, Session.Manager))
                {
                    OpenChildForm(new FrmDamagereport());
                }
            };

            ToolStripMenuItem low = new ToolStripMenuItem("Low Stock");
            low.Name = "lowStockToolStripMenuItem";
            low.Click += delegate
            {
                if (Allow(Session.InventoryStaff, Session.Manager))
                {
                    OpenChildForm(new frmLowStock());
                }
            };

            toolStripDropDownButton2.DropDownItems.AddRange(new ToolStripItem[] { sales, low, expired, damage });

            ToolStripMenuItem suppliers = new ToolStripMenuItem("Suppliers");
            suppliers.Click += delegate
            {
                if (Allow(Session.InventoryStaff, Session.Manager))
                {
                    OpenChildForm(new frmSupplierManager());
                }
            };

            ToolStripMenuItem categories = new ToolStripMenuItem("Categories");
            categories.Click += delegate
            {
                if (Allow(Session.InventoryStaff, Session.Manager))
                {
                    OpenChildForm(new frmCategoryManager());
                }
            };

            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { suppliers, categories });

            // Every logged-in user may change their own password.
            ToolStripMenuItem pwd = new ToolStripMenuItem("Change Password");
            pwd.Click += delegate
            {
                using (frmChangePassword dlg = new frmChangePassword())
                {
                    dlg.ShowDialog(this);
                }
            };

            int at = toolStripDropDownButton3.DropDownItems.IndexOf(logoutToolStripMenuItem);
            toolStripDropDownButton3.DropDownItems.Insert(at < 0 ? 0 : at, pwd);
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Log out of " + Session.AccountName + "?", "Logout",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            LogoutRequested = true;
            Session.End();
            Close();
        }

        // Adds  Setting > Security Tools  in code, so the designer file is not touched.
        private void AddSecurityToolsMenu()
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Security Tools");
            item.Name = "securityToolsToolStripMenuItem";
            item.Click += securityToolsToolStripMenuItem_Click;

            // Right after "Account Management".
            int index = toolStripDropDownButton3.DropDownItems.IndexOf(accountManagementToolStripMenuItem);
            toolStripDropDownButton3.DropDownItems.Insert(index + 1, item);
        }

        private void securityToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (frmSecurityTools tools = new frmSecurityTools())
            {
                tools.ShowDialog(this);
            }
        }

        private void frmMainform_Load(object sender, EventArgs e)
        {
            if (Session.IsLoggedIn)
            {
                Text = Text + "  -  " + Session.AccountName + " (" + Session.AccountType + ")";
            }

            ShowDashboard();
        }

        // Role check. Admin always passes. Roles come from tbl_account.acc_type.
        private bool Allow(params string[] roles)
        {
            if (Session.Can(roles))
            {
                return true;
            }

            MessageBox.Show(
                "Your account type (" + Session.AccountType + ") is not allowed to open this screen.",
                "Access denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        // ==========================================
        // Loads any form inside panel2 as a child form
        // ==========================================
        private void OpenChildForm(Form child)
        {
            if (_currentChild != null)
            {
                _currentChild.Close();
                _currentChild.Dispose();
            }

            _currentChild = child;

            child.TopLevel = false;
            child.FormBorderStyle = FormBorderStyle.None;
            child.Dock = DockStyle.Fill;

            panel2.Controls.Clear();
            panel2.Controls.Add(child);

            child.Show();
            child.BringToFront();
        }

        private void ShowDashboard()
        {
            OpenChildForm(new frmDashboard());
        }

        private void timer1_Tick(object sender, EventArgs e) { }

        private void panel1_Paint(object sender, PaintEventArgs e) { }

        private void toolStripButton1_Click(object sender, EventArgs e)   // Dashboard
        {
            ShowDashboard();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)   // Home
        {
            ShowDashboard();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)   // Point of Sale
        {
            if (!Allow(Session.Cashier, Session.Manager))
            {
                return;
            }

            OpenChildForm(new FrmSales());
        }

        private void manageProductToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Allow(Session.InventoryStaff, Session.Manager))
            {
                return;
            }

            OpenChildForm(new frmproduct());
        }

        private void manageStocksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Allow(Session.InventoryStaff, Session.Manager))
            {
                return;
            }

            OpenChildForm(new frmStocks());
        }

        private void toolStripDropDownButton2_Click(object sender, EventArgs e)   // Report
        {
            // OpenChildForm(new frmReport(_db));
        }

        private void toolStripDropDownButton3_Click(object sender, EventArgs e) { }   // Setting

        private void accountManagementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Allow())   // Admin only
            {
                return;
            }

            frmAccount account = new frmAccount(_db);
            account.ShowDialog();
        }

        private void exitAppToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}