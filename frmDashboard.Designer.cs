namespace inventory_management_system_with_POS
{
    partial class frmDashboard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void intialiazationcomponent()
        {
            this.lblSalesByCategory = new System.Windows.Forms.Label();
            this.lblSnacks = new System.Windows.Forms.Label();
            this.lblBeverage = new System.Windows.Forms.Label();
            this.lblHousehold = new System.Windows.Forms.Label();
            this.lblCancelled = new System.Windows.Forms.Label();
            this.txtSnacks = new System.Windows.Forms.TextBox();
            this.txtBeverage = new System.Windows.Forms.TextBox();
            this.txtHousehold = new System.Windows.Forms.TextBox();
            this.txtCancelled = new System.Windows.Forms.TextBox();
            this.lblDateRange = new System.Windows.Forms.Label();
            this.lblFrom = new System.Windows.Forms.Label();
            this.lblTo = new System.Windows.Forms.Label();
            this.txtFrom = new System.Windows.Forms.TextBox();
            this.txtTo = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.dgvSales = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSales)).BeginInit();
            this.SuspendLayout();
            // 
            // lblSalesByCategory
            // 
            this.lblSalesByCategory.BackColor = System.Drawing.Color.MistyRose;
            this.lblSalesByCategory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblSalesByCategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblSalesByCategory.Location = new System.Drawing.Point(24, 57);
            this.lblSalesByCategory.Name = "lblSalesByCategory";
            this.lblSalesByCategory.Size = new System.Drawing.Size(235, 45);
            this.lblSalesByCategory.TabIndex = 0;
            this.lblSalesByCategory.Text = "SALES BY CATEGORY:";
            this.lblSalesByCategory.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSnacks
            // 
            this.lblSnacks.AutoSize = true;
            this.lblSnacks.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblSnacks.Location = new System.Drawing.Point(240, 125);
            this.lblSnacks.Name = "lblSnacks";
            this.lblSnacks.Size = new System.Drawing.Size(84, 25);
            this.lblSnacks.TabIndex = 1;
            this.lblSnacks.Text = "Snacks:";
            // 
            // lblBeverage
            // 
            this.lblBeverage.AutoSize = true;
            this.lblBeverage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblBeverage.Location = new System.Drawing.Point(240, 169);
            this.lblBeverage.Name = "lblBeverage";
            this.lblBeverage.Size = new System.Drawing.Size(102, 25);
            this.lblBeverage.TabIndex = 3;
            this.lblBeverage.Text = "Beverage:";
            // 
            // lblHousehold
            // 
            this.lblHousehold.AutoSize = true;
            this.lblHousehold.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblHousehold.Location = new System.Drawing.Point(240, 218);
            this.lblHousehold.Name = "lblHousehold";
            this.lblHousehold.Size = new System.Drawing.Size(112, 25);
            this.lblHousehold.TabIndex = 5;
            this.lblHousehold.Text = "Household:";
            // 
            // lblCancelled
            // 
            this.lblCancelled.AutoSize = true;
            this.lblCancelled.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblCancelled.Location = new System.Drawing.Point(240, 262);
            this.lblCancelled.Name = "lblCancelled";
            this.lblCancelled.Size = new System.Drawing.Size(106, 25);
            this.lblCancelled.TabIndex = 7;
            this.lblCancelled.Text = "Cancelled:";
            // 
            // txtSnacks
            // 
            this.txtSnacks.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtSnacks.Location = new System.Drawing.Point(358, 124);
            this.txtSnacks.Name = "txtSnacks";
            this.txtSnacks.ReadOnly = true;
            this.txtSnacks.Size = new System.Drawing.Size(247, 28);
            this.txtSnacks.TabIndex = 2;
            // 
            // txtBeverage
            // 
            this.txtBeverage.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtBeverage.Location = new System.Drawing.Point(358, 169);
            this.txtBeverage.Name = "txtBeverage";
            this.txtBeverage.ReadOnly = true;
            this.txtBeverage.Size = new System.Drawing.Size(247, 28);
            this.txtBeverage.TabIndex = 4;
            // 
            // txtHousehold
            // 
            this.txtHousehold.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtHousehold.Location = new System.Drawing.Point(358, 215);
            this.txtHousehold.Name = "txtHousehold";
            this.txtHousehold.ReadOnly = true;
            this.txtHousehold.Size = new System.Drawing.Size(247, 28);
            this.txtHousehold.TabIndex = 6;
            // 
            // txtCancelled
            // 
            this.txtCancelled.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtCancelled.Location = new System.Drawing.Point(358, 257);
            this.txtCancelled.Name = "txtCancelled";
            this.txtCancelled.ReadOnly = true;
            this.txtCancelled.Size = new System.Drawing.Size(247, 28);
            this.txtCancelled.TabIndex = 8;
            // 
            // lblDateRange
            // 
            this.lblDateRange.BackColor = System.Drawing.Color.MistyRose;
            this.lblDateRange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDateRange.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblDateRange.Location = new System.Drawing.Point(621, 57);
            this.lblDateRange.Name = "lblDateRange";
            this.lblDateRange.Size = new System.Drawing.Size(211, 45);
            this.lblDateRange.TabIndex = 9;
            this.lblDateRange.Text = "Date Range";
            this.lblDateRange.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblFrom.Location = new System.Drawing.Point(790, 151);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(63, 25);
            this.lblFrom.TabIndex = 10;
            this.lblFrom.Text = "From:";
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblTo.Location = new System.Drawing.Point(790, 230);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(42, 25);
            this.lblTo.TabIndex = 12;
            this.lblTo.Text = "To:";
            // 
            // txtFrom
            // 
            this.txtFrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtFrom.Location = new System.Drawing.Point(837, 176);
            this.txtFrom.Name = "txtFrom";
            this.txtFrom.Size = new System.Drawing.Size(235, 28);
            this.txtFrom.TabIndex = 11;
            // 
            // txtTo
            // 
            this.txtTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtTo.Location = new System.Drawing.Point(837, 257);
            this.txtTo.Name = "txtTo";
            this.txtTo.Size = new System.Drawing.Size(235, 28);
            this.txtTo.TabIndex = 13;
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lblSearch.Location = new System.Drawing.Point(34, 340);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(81, 25);
            this.lblSearch.TabIndex = 14;
            this.lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.txtSearch.Location = new System.Drawing.Point(137, 334);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(525, 28);
            this.txtSearch.TabIndex = 15;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // dgvSales
            // 
            this.dgvSales.AllowUserToDeleteRows = false;
            this.dgvSales.AllowUserToResizeRows = false;
            this.dgvSales.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSales.BackgroundColor = System.Drawing.Color.DarkGray;
            this.dgvSales.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSales.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn5,
            this.dataGridViewTextBoxColumn6,
            this.dataGridViewTextBoxColumn7,
            this.dataGridViewTextBoxColumn8});
            this.dgvSales.Location = new System.Drawing.Point(137, 418);
            this.dgvSales.MultiSelect = false;
            this.dgvSales.Name = "dgvSales";
            this.dgvSales.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.dgvSales.RowHeadersWidth = 51;
            this.dgvSales.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSales.Size = new System.Drawing.Size(935, 291);
            this.dgvSales.TabIndex = 16;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.HeaderText = "Receipt No.";
            this.dataGridViewTextBoxColumn5.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.HeaderText = "Item Count";
            this.dataGridViewTextBoxColumn6.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.HeaderText = "Total";
            this.dataGridViewTextBoxColumn7.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            // 
            // dataGridViewTextBoxColumn8
            // 
            this.dataGridViewTextBoxColumn8.HeaderText = "Time";
            this.dataGridViewTextBoxColumn8.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            // 
            // frmDashboard
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(196)))), ((int)(((byte)(222)))));
            this.ClientSize = new System.Drawing.Size(1150, 750);
            this.Controls.Add(this.lblSalesByCategory);
            this.Controls.Add(this.lblSnacks);
            this.Controls.Add(this.txtSnacks);
            this.Controls.Add(this.lblBeverage);
            this.Controls.Add(this.txtBeverage);
            this.Controls.Add(this.lblHousehold);
            this.Controls.Add(this.txtHousehold);
            this.Controls.Add(this.lblCancelled);
            this.Controls.Add(this.txtCancelled);
            this.Controls.Add(this.lblDateRange);
            this.Controls.Add(this.lblFrom);
            this.Controls.Add(this.txtFrom);
            this.Controls.Add(this.lblTo);
            this.Controls.Add(this.txtTo);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.dgvSales);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmDashboard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmDashboard";
            ((System.ComponentModel.ISupportInitialize)(this.dgvSales)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.MaskedTextBox maskedTextBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.MaskedTextBox maskedTextBox2;
        private System.Windows.Forms.MaskedTextBox maskedTextBox3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label8;
    }
}