namespace inventory_management_system_with_POS
{
    // Kept on purpose almost empty: the screen is built in code by
    // frmReportBase (see ReportBase.cs).
    partial class frmExpireditem
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
            this.SuspendLayout();
            // 
            // frmExpireditem
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1100, 640);
            this.Name = "frmExpireditem";
            this.Text = "Expired Items";
            this.ResumeLayout(false);

        }

        #endregion
    }
}
