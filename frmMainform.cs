
using System;
using System.Windows.Forms;

namespace inventory_management_system_with_POS
{
    public partial class frmMainform : Form
    {
        private readonly MySQL_Dataabase _db;
        private Form _currentChild;

        public bool LogoutRequested { get; private set; }

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

            // DATE AND TIME
            UpdateDateTime();

            timer1.Interval = 1000;
            timer1.Start();
        }

        // ==========================================
        // DATE AND TIME
        // ==========================================

        private void UpdateDateTime()
        {
            label2.Text = DateTime.Now.ToString("MMMM dd, yyyy");

            label3.Text = DateTime.Now.ToString("hh:mm:ss tt");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        // ==========================================
        // MAIN FORM LOAD
        // ==========================================

        private void frmMainform_Load(object sender, EventArgs e)
        {
            if (Session.IsLoggedIn)
            {
                Text = Text + "  -  " +
                    Session.AccountName + " (" +
                    Session.AccountType + ")";
            }

            UpdateDateTime();

            ShowDashboard();
        }

        // ==========================================
        // REPORT MENUS
        // ==========================================

        private void AddReportMenus()
        {
            ToolStripMenuItem sales =
                new ToolStripMenuItem("Sales Report");

            sales.Name = "salesReportToolStripMenuItem";

            sales.Click += delegate
            {
                if (Allow(Session.Manager))
                {
                    OpenChildForm(new frmManageReport());
                }
            };

            ToolStripMenuItem expired =
                new ToolStripMenuItem("Expired Items");

            expired.Name = "expiredItemsToolStripMenuItem";

            expired.Click += delegate
            {
                if (Allow(Session.InventoryStaff, Session.Manager))
                {
                    OpenChildForm(new frmExpireditem());
                }
            };

            ToolStripMenuItem damage =
                new ToolStripMenuItem("Damage Report");

            damage.Name = "damageReportToolStripMenuItem";

            damage.Click += delegate
            {
                if (Allow(Session.InventoryStaff, Session.Manager))
                {
                    OpenChildForm(new FrmDamagereport());
                }
            };

            ToolStripMenuItem low =
                new ToolStripMenuItem("Low Stock");

            low.Name = "lowStockToolStripMenuItem";

            low.Click += delegate
            {
                if (Allow(Session.InventoryStaff, Session.Manager))
                {
                    OpenChildForm(new frmLowStock());
                }
            };

            toolStripDropDownButton2.DropDownItems.AddRange(
                new ToolStripItem[] { sales, low, expired, damage });

            ToolStripMenuItem suppliers =
                new ToolStripMenuItem("Suppliers");

            suppliers.Click += delegate
            {
                if (Allow(Session.InventoryStaff, Session.Manager))
                {
                    OpenChildForm(new frmSupplierManager());
                }
            };

            ToolStripMenuItem categories =
                new ToolStripMenuItem("Categories");

            categories.Click += delegate
            {
                if (Allow(Session.InventoryStaff, Session.Manager))
                {
                    OpenChildForm(new frmCategoryManager());
                }
            };

            toolStripDropDownButton1.DropDownItems.AddRange(
                new ToolStripItem[] { suppliers, categories });

            ToolStripMenuItem pwd =
                new ToolStripMenuItem("Change Password");

            pwd.Click += delegate
            {
                using (frmChangePassword dlg =
                    new frmChangePassword())
                {
                    dlg.ShowDialog(this);
                }
            };

            int at =
                toolStripDropDownButton3.DropDownItems.IndexOf(
                    logoutToolStripMenuItem);

            toolStripDropDownButton3.DropDownItems.Insert(
                at < 0 ? 0 : at, pwd);
        }

        // ==========================================
        // SECURITY TOOLS
        // ==========================================

        private void AddSecurityToolsMenu()
        {
            ToolStripMenuItem item =
                new ToolStripMenuItem("Security Tools");

            item.Name = "securityToolsToolStripMenuItem";

            item.Click += securityToolsToolStripMenuItem_Click;

            int index =
                toolStripDropDownButton3.DropDownItems.IndexOf(
                    accountManagementToolStripMenuItem);

            toolStripDropDownButton3.DropDownItems.Insert(
                index + 1, item);
        }

        private void securityToolsToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            using (frmSecurityTools tools =
                new frmSecurityTools())
            {
                tools.ShowDialog(this);
            }
        }

        // ==========================================
        // LOGOUT
        // ==========================================

        private void logoutToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "Log out of " + Session.AccountName + "?",
                "Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)
                != DialogResult.Yes)
            {
                return;
            }

            LogoutRequested = true;

            Session.End();

            Close();
        }

        // ==========================================
        // ROLE ACCESS
        // ==========================================

        private bool Allow(params string[] roles)
        {
            if (Session.Can(roles))
            {
                return true;
            }

            MessageBox.Show(
                "Your account type (" +
                Session.AccountType +
                ") is not allowed to open this screen.",
                "Access denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        // ==========================================
        // OPEN CHILD FORM
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

            child.FormBorderStyle =
                FormBorderStyle.None;

            child.Dock = DockStyle.Fill;

            panel2.Controls.Clear();

            panel2.Controls.Add(child);

            child.Show();

            child.BringToFront();
        }

        // ==========================================
        // DASHBOARD
        // ==========================================

        private void ShowDashboard()
        {
            OpenChildForm(new frmDashboard());
        }

        private void toolStripButton1_Click(
            object sender, EventArgs e)
        {
            ShowDashboard();
        }

        // ==========================================
        // HOME
        // ==========================================

        private void toolStripButton2_Click(
            object sender, EventArgs e)
        {
            if (_currentChild != null)
            {
                _currentChild.Close();
                _currentChild.Dispose();

                _currentChild = null;
            }

            panel2.Controls.Clear();
        }

        // ==========================================
        // POINT OF SALE
        // ==========================================

        private void toolStripButton3_Click(
            object sender, EventArgs e)
        {
            if (!Allow(Session.Cashier, Session.Manager))
            {
                return;
            }

            OpenChildForm(
                new FrmPOS(_db != null ? _db.conn : null));
        }

        // ==========================================
        // MANAGE PRODUCTS
        // ==========================================

        private void manageProductToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            if (!Allow(Session.InventoryStaff, Session.Manager))
            {
                return;
            }

            OpenChildForm(new frmproduct());
        }

        // ==========================================
        // MANAGE STOCKS
        // ==========================================

        private void manageStocksToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            if (!Allow(Session.InventoryStaff, Session.Manager))
            {
                return;
            }

            OpenChildForm(new frmStocks());
        }

        // ==========================================
        // REPORT
        // ==========================================

        private void toolStripDropDownButton2_Click(
            object sender, EventArgs e)
        {
        }

        // ==========================================
        // SETTING
        // ==========================================

        private void toolStripDropDownButton3_Click(
            object sender, EventArgs e)
        {
        }

        // ==========================================
        // ACCOUNT MANAGEMENT
        // ==========================================

        private void accountManagementToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            if (!Allow())
            {
                return;
            }

            frmAccount account = new frmAccount(_db);

            account.ShowDialog();
        }

        // ==========================================
        // EXIT APPLICATION
        // ==========================================

        private void exitAppToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            Application.Exit();
        }

        // ==========================================
        // LABEL EVENTS
        // ==========================================

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void panel1_Paint(
            object sender, PaintEventArgs e)
        {
        }
    }
}
