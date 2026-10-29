using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace inventory_management_system_with_POS
{
    public partial class frmDashboard : Form
    {
        private DataTable productTable;
        private DataTable soldTable;

        public frmDashboard()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.Dock = DockStyle.Fill;

            // cards are positioned by LayoutCards, so anchor them top-left only
            foreach (Panel card in new[] { panel2, panel3, panel4, panel5 })
                card.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            this.Resize += (s, e) => LayoutCards();

            SetupGrids();
            LoadSampleData();
            UpdateDashboard();
        }

        // =====================================
        // DASHBOARD LOAD
        // =====================================

        private void frmDashboard_Load(object sender, EventArgs e)
        {
            LayoutCards();
            UpdateDashboard();
        }

        // =====================================
        // LAYOUT SUMMARY CARDS (equal width, equal gaps)
        // =====================================

        private void LayoutCards()
        {
            int margin = 36;   // space at left and right edges
            int gap = 20;      // space between cards
            int top = 103;
            int height = 78;

            int cardWidth = (this.ClientSize.Width - (margin * 2) - (gap * 3)) / 4;
            if (cardWidth < 50) return;

            Panel[] cards = { panel2, panel3, panel4, panel5 };

            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].SetBounds(
                    margin + i * (cardWidth + gap),
                    top,
                    cardWidth,
                    height);
            }
        }

        // =====================================
        // GRID SETUP (bind designer columns)
        // =====================================

        private void SetupGrids()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView2.AutoGenerateColumns = false;

            // Recent Sold
            Column1.DataPropertyName = "ID";
            Column2.DataPropertyName = "ProductName";
            Column3.DataPropertyName = "Category";
            Column4.DataPropertyName = "Stock";
            Column5.DataPropertyName = "Price";
            Column5.DefaultCellStyle.Format = "₱#,##0.00";

            // Low Stock
            dataGridViewTextBoxColumn1.DataPropertyName = "ID";
            dataGridViewTextBoxColumn2.DataPropertyName = "ProductName";
            dataGridViewTextBoxColumn3.DataPropertyName = "Category";
            dataGridViewTextBoxColumn4.DataPropertyName = "Stock";
            dataGridViewTextBoxColumn5.DataPropertyName = "Price";
            dataGridViewTextBoxColumn5.DefaultCellStyle.Format = "₱#,##0.00";
        }

        // =====================================
        // SAMPLE DATA
        // =====================================

        private void LoadSampleData()
        {
            productTable = new DataTable();

            productTable.Columns.Add("ID");
            productTable.Columns.Add("ProductName");
            productTable.Columns.Add("Category");
            productTable.Columns.Add("Stock", typeof(int));
            productTable.Columns.Add("Price", typeof(decimal));

            productTable.Rows.Add("P001", "Coca Cola", "Drinks", 5, 20.00m);
            productTable.Rows.Add("P002", "Bread", "Food", 50, 15.00m);
            productTable.Rows.Add("P003", "Soap", "Personal Care", 3, 35.00m);
            productTable.Rows.Add("P004", "Rice", "Grocery", 100, 55.00m);
            productTable.Rows.Add("P005", "Juice", "Drinks", 8, 25.00m);

            soldTable = new DataTable();

            soldTable.Columns.Add("ID");
            soldTable.Columns.Add("ProductName");
            soldTable.Columns.Add("Category");
            soldTable.Columns.Add("Stock", typeof(int));
            soldTable.Columns.Add("Price", typeof(decimal));

            soldTable.Rows.Add("P002", "Bread", "Food", 1, 15.00m);
            soldTable.Rows.Add("P001", "Coca Cola", "Drinks", 2, 20.00m);
        }

        // =====================================
        // UPDATE DASHBOARD
        // =====================================

        private void UpdateDashboard()
        {
            if (productTable == null || soldTable == null)
                return;

            int totalStocks = 0;
            int lowStockCount = 0;
            DataTable lowStockTable = productTable.Clone();

            foreach (DataRow row in productTable.Rows)
            {
                int stock = Convert.ToInt32(row["Stock"]);
                totalStocks += stock;

                if (stock <= 10)
                {
                    lowStockCount++;
                    lowStockTable.ImportRow(row);
                }
            }

            label8.Text = productTable.Rows.Count.ToString();  // Total Products
            label9.Text = totalStocks.ToString();               // Total Stocks
            label10.Text = productTable.Rows.Count.ToString();   // Total Stocks items
            label11.Text = lowStockCount.ToString();             // Low Stocks items

            dataGridView1.DataSource = soldTable;                // Recent Sold
            dataGridView2.DataSource = lowStockTable;            // Low Stock
        }

        // =====================================
        // REFRESH DASHBOARD
        // =====================================

        public void RefreshDashboard()
        {
            UpdateDashboard();
        }
    }
}