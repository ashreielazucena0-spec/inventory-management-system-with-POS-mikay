namespace inventory_management_system_with_POS
{
    partial class frmMainform
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(frmMainform));

            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();

            this.timer1 = new System.Windows.Forms.Timer(this.components);

            this.toolStrip1 = new System.Windows.Forms.ToolStrip();

            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();

            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.manageProductToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageStocksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();

            this.toolStripDropDownButton3 = new System.Windows.Forms.ToolStripDropDownButton();
            this.accountManagementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitAppToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.panel2 = new System.Windows.Forms.Panel();

            this.panel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();

            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.HotPink;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.ForeColor = System.Drawing.Color.White;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1801, 99);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);

            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 23);
            this.label3.TabIndex = 1;
            this.label3.Text = "label3";

            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font(
                "Tahoma",
                24F,
                System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point,
                ((byte)(0))
            );
            this.label2.Location = new System.Drawing.Point(18, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 48);
            this.label2.TabIndex = 0;
            this.label2.Text = "label2";

            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);

            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);

            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                this.toolStripButton2,
                this.toolStripButton1,
                this.toolStripButton3,
                this.toolStripDropDownButton1,
                this.toolStripDropDownButton2,
                this.toolStripDropDownButton3
            });

            this.toolStrip1.Location = new System.Drawing.Point(0, 99);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1801, 31);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";

            // 
            // toolStripButton2 - HOME
            // 
            this.toolStripButton2.Image =
                global::inventory_management_system_with_POS.Properties.Resources.home;

            this.toolStripButton2.ImageTransparentColor =
                System.Drawing.Color.Magenta;

            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(78, 28);
            this.toolStripButton2.Text = "Home";

            this.toolStripButton2.Click +=
                new System.EventHandler(this.toolStripButton2_Click);

            // 
            // toolStripButton1 - DASHBOARD
            // 
            this.toolStripButton1.Image =
                global::inventory_management_system_with_POS.Properties.Resources.grid;

            this.toolStripButton1.ImageTransparentColor =
                System.Drawing.Color.Magenta;

            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(110, 28);
            this.toolStripButton1.Text = "Dashboard";

            this.toolStripButton1.Click +=
                new System.EventHandler(this.toolStripButton1_Click);

            // 
            // toolStripButton3 - POINT OF SALE
            // 
            this.toolStripButton3.Image =
                global::inventory_management_system_with_POS.Properties.Resources.point_of_sale;

            this.toolStripButton3.ImageTransparentColor =
                System.Drawing.Color.Magenta;

            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(120, 28);
            this.toolStripButton3.Text = "Point of Sale";

            this.toolStripButton3.Click +=
                new System.EventHandler(this.toolStripButton3_Click);

            // 
            // toolStripDropDownButton1 - MANAGEMENT
            // 
            this.toolStripDropDownButton1.DropDownItems.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    this.manageProductToolStripMenuItem,
                    this.manageStocksToolStripMenuItem
                });

            this.toolStripDropDownButton1.Image =
                global::inventory_management_system_with_POS.Properties.Resources.people;

            this.toolStripDropDownButton1.ImageTransparentColor =
                System.Drawing.Color.Magenta;

            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(135, 28);
            this.toolStripDropDownButton1.Text = "Management";

            // 
            // manageProductToolStripMenuItem
            // 
            this.manageProductToolStripMenuItem.Name =
                "manageProductToolStripMenuItem";

            this.manageProductToolStripMenuItem.Size =
                new System.Drawing.Size(201, 26);

            this.manageProductToolStripMenuItem.Text =
                "Manage Product";

            this.manageProductToolStripMenuItem.Click +=
                new System.EventHandler(this.manageProductToolStripMenuItem_Click);

            // 
            // manageStocksToolStripMenuItem
            // 
            this.manageStocksToolStripMenuItem.Name =
                "manageStocksToolStripMenuItem";

            this.manageStocksToolStripMenuItem.Size =
                new System.Drawing.Size(201, 26);

            this.manageStocksToolStripMenuItem.Text =
                "Manage Stocks";

            this.manageStocksToolStripMenuItem.Click +=
                new System.EventHandler(this.manageStocksToolStripMenuItem_Click);

            // 
            // toolStripDropDownButton2 - REPORT
            // 
            this.toolStripDropDownButton2.Image =
                ((System.Drawing.Image)(resources.GetObject(
                    "toolStripDropDownButton2.Image"
                )));

            this.toolStripDropDownButton2.ImageTransparentColor =
                System.Drawing.Color.Magenta;

            this.toolStripDropDownButton2.Name =
                "toolStripDropDownButton2";

            this.toolStripDropDownButton2.Size =
                new System.Drawing.Size(92, 28);

            this.toolStripDropDownButton2.Text = "Report";

            this.toolStripDropDownButton2.Click +=
                new System.EventHandler(this.toolStripDropDownButton2_Click);

            // 
            // toolStripDropDownButton3 - SETTING
            // 
            this.toolStripDropDownButton3.DropDownItems.AddRange(
                new System.Windows.Forms.ToolStripItem[]
                {
                    this.accountManagementToolStripMenuItem,
                    this.exitAppToolStripMenuItem,
                    this.logoutToolStripMenuItem
                });

            this.toolStripDropDownButton3.Image =
                ((System.Drawing.Image)(resources.GetObject(
                    "toolStripDropDownButton3.Image"
                )));

            this.toolStripDropDownButton3.ImageTransparentColor =
                System.Drawing.Color.Magenta;

            this.toolStripDropDownButton3.Name =
                "toolStripDropDownButton3";

            this.toolStripDropDownButton3.Size =
                new System.Drawing.Size(94, 28);

            this.toolStripDropDownButton3.Text = "Setting";

            this.toolStripDropDownButton3.Click +=
                new System.EventHandler(this.toolStripDropDownButton3_Click);

            // 
            // accountManagementToolStripMenuItem
            // 
            this.accountManagementToolStripMenuItem.Name =
                "accountManagementToolStripMenuItem";

            this.accountManagementToolStripMenuItem.Size =
                new System.Drawing.Size(238, 26);

            this.accountManagementToolStripMenuItem.Text =
                "Account Management";

            this.accountManagementToolStripMenuItem.Click +=
                new System.EventHandler(this.accountManagementToolStripMenuItem_Click);

            // 
            // exitAppToolStripMenuItem
            // 
            this.exitAppToolStripMenuItem.Name =
                "exitAppToolStripMenuItem";

            this.exitAppToolStripMenuItem.Size =
                new System.Drawing.Size(238, 26);

            this.exitAppToolStripMenuItem.Text =
                "Exit app";

            this.exitAppToolStripMenuItem.Click +=
                new System.EventHandler(this.exitAppToolStripMenuItem_Click);

            // 
            // logoutToolStripMenuItem
            // 
            this.logoutToolStripMenuItem.Name =
                "logoutToolStripMenuItem";

            this.logoutToolStripMenuItem.Size =
                new System.Drawing.Size(238, 26);

            this.logoutToolStripMenuItem.Text =
                "Logout";

            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize =
                new System.Drawing.Size(20, 20);

            this.statusStrip1.Location =
                new System.Drawing.Point(0, 681);

            this.statusStrip1.Name =
                "statusStrip1";

            this.statusStrip1.Size =
                new System.Drawing.Size(1801, 22);

            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";

            // 
            // panel2
            // IMPORTANT: Fill instead of Left
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 130);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1801, 551);
            this.panel2.TabIndex = 4;

            // 
            // frmMainform
            // 
            this.AutoScaleDimensions =
                new System.Drawing.SizeF(12F, 23F);

            this.AutoScaleMode =
                System.Windows.Forms.AutoScaleMode.Font;

            this.ClientSize =
                new System.Drawing.Size(1801, 703);

            this.ControlBox = false;

            this.Controls.Add(this.panel2);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.panel1);

            this.Font =
                new System.Drawing.Font(
                    "Century",
                    11.25F,
                    System.Drawing.FontStyle.Regular,
                    System.Drawing.GraphicsUnit.Point,
                    ((byte)(0))
                );

            this.Margin =
                new System.Windows.Forms.Padding(4);

            this.Name = "frmMainform";

            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterScreen;

            this.WindowState =
                System.Windows.Forms.FormWindowState.Maximized;

            this.Load +=
                new System.EventHandler(this.frmMainform_Load);

            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();

            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer timer1;

        private System.Windows.Forms.ToolStrip toolStrip1;

        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton3;

        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem manageProductToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageStocksToolStripMenuItem;

        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;

        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton3;
        private System.Windows.Forms.ToolStripMenuItem accountManagementToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitAppToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logoutToolStripMenuItem;

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel2;
    }
}