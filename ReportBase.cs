using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace inventory_management_system_with_POS
{
    // ==========================================================
    // REPORT SCREEN BASE
    //
    // Common layout for  Sales Report, Expired Items and Damage Report:
    //
    //   [ title bar                                  ]
    //   [ filters: From / To / Search / extra filters ]
    //   [ summary line                               ]
    //   [ grid                                       ]
    //   [ buttons: Refresh / Export CSV / extra       ]
    //
    // A report form derives from this class, calls the constructor,
    // adds its own buttons / filters and overrides LoadData().
    // Everything is built in code, so the designer files stay tiny.
    // ==========================================================
    public class frmReportBase : Form
    {
        protected const string Peso = "\u20B1";

        protected readonly Label TitleLabel;
        protected readonly FlowLayoutPanel FilterBar;
        protected readonly Label SummaryLabel;
        protected readonly DataGridView Grid;
        protected readonly FlowLayoutPanel ButtonBar;

        protected readonly DateTimePicker DtpFrom;
        protected readonly DateTimePicker DtpTo;
        protected readonly TextBox TxtSearch;

        private readonly string _csvName;
        private bool _suspendReload;

        public frmReportBase() : this("REPORT", "report", true)
        {
        }

        protected frmReportBase(string title, string csvName, bool showDates)
        {
            _csvName = csvName;

            Text = title;
            BackColor = Color.FromArgb(174, 196, 222);
            Font = new Font("Segoe UI", 9F);
            ClientSize = new Size(1100, 640);

            // ---- title bar ----
            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 52;
            header.BackColor = Color.DarkGray;

            TitleLabel = new Label();
            TitleLabel.Text = title;
            TitleLabel.AutoSize = true;
            TitleLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            TitleLabel.Location = new Point(30, 15);
            header.Controls.Add(TitleLabel);

            // ---- filter bar ----
            FilterBar = new FlowLayoutPanel();
            FilterBar.Dock = DockStyle.Top;
            FilterBar.Height = 46;
            FilterBar.Padding = new Padding(20, 8, 0, 0);
            FilterBar.WrapContents = false;

            DtpFrom = new DateTimePicker();
            DtpTo = new DateTimePicker();
            DtpFrom.Format = DateTimePickerFormat.Short;
            DtpTo.Format = DateTimePickerFormat.Short;
            DtpFrom.Width = 110;
            DtpTo.Width = 110;
            DtpFrom.Value = DateTime.Today.AddDays(-30);
            DtpTo.Value = DateTime.Today;
            DtpFrom.ValueChanged += Filter_Changed;
            DtpTo.ValueChanged += Filter_Changed;

            if (showDates)
            {
                AddFilter("From:", DtpFrom);
                AddFilter("To:", DtpTo);
            }

            TxtSearch = new TextBox();
            TxtSearch.Width = 190;
            TxtSearch.KeyDown += TxtSearch_KeyDown;
            AddFilter("Search:", TxtSearch);

            // ---- summary line ----
            SummaryLabel = new Label();
            SummaryLabel.Dock = DockStyle.Top;
            SummaryLabel.Height = 34;
            SummaryLabel.Padding = new Padding(22, 0, 0, 0);
            SummaryLabel.TextAlign = ContentAlignment.MiddleLeft;
            SummaryLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            // ---- grid ----
            Grid = CreateGrid();
            Grid.Dock = DockStyle.Fill;

            // ---- button bar ----
            ButtonBar = new FlowLayoutPanel();
            ButtonBar.Dock = DockStyle.Bottom;
            ButtonBar.Height = 54;
            ButtonBar.Padding = new Padding(20, 10, 0, 0);
            ButtonBar.WrapContents = false;

            AddButton("Refresh", delegate { Reload(); });
            AddButton("Export CSV", delegate { ExportCsv(); });

            // Add order = dock order: the grid (Fill) first, the outer bars last.
            Controls.Add(Grid);
            Controls.Add(ButtonBar);
            Controls.Add(SummaryLabel);
            Controls.Add(FilterBar);
            Controls.Add(header);

            Load += delegate { Reload(); };
        }

        // ------------------------------------------------------------
        // Building blocks for the derived forms
        // ------------------------------------------------------------
        protected void AddFilter(string caption, Control input)
        {
            Label l = new Label();
            l.Text = caption;
            l.AutoSize = false;
            l.Size = new Size(caption.Length * 8 + 14, 26);
            l.TextAlign = ContentAlignment.MiddleRight;
            FilterBar.Controls.Add(l);

            input.Margin = new Padding(3, 3, 14, 3);
            FilterBar.Controls.Add(input);

            ComboBox combo = input as ComboBox;

            if (combo != null)
            {
                combo.SelectedIndexChanged += Filter_Changed;
            }
        }

        protected Button AddButton(string text, EventHandler onClick)
        {
            Button b = new Button();
            b.Text = text;
            b.Size = new Size(text.Length > 14 ? 170 : 120, 34);
            b.Margin = new Padding(0, 0, 10, 0);
            b.BackColor = SystemColors.ActiveCaption;
            b.UseVisualStyleBackColor = false;
            b.Click += onClick;
            ButtonBar.Controls.Add(b);
            return b;
        }

        protected static DataGridView CreateGrid()
        {
            DataGridView g = new DataGridView();
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.MultiSelect = false;
            g.RowHeadersVisible = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.FixedSingle;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
            return g;
        }

        // Header text, number/date format and alignment for one result column.
        // Safe to call for a column that does not exist.
        protected static void Format(DataGridView grid, string column, string header, string format, bool alignRight)
        {
            if (!grid.Columns.Contains(column))
            {
                return;
            }

            DataGridViewColumn col = grid.Columns[column];
            col.HeaderText = header;

            if (!string.IsNullOrEmpty(format))
            {
                col.DefaultCellStyle.Format = format;
            }

            if (alignRight)
            {
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        protected static void HideColumn(DataGridView grid, string column)
        {
            if (grid.Columns.Contains(column))
            {
                grid.Columns[column].Visible = false;
            }
        }

        protected static decimal Dec(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0m;
            }

            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        protected static int Int(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        // The grid row's underlying DataRow (null when nothing is selected).
        protected DataRow SelectedRow()
        {
            if (Grid.CurrentRow == null)
            {
                return null;
            }

            DataRowView view = Grid.CurrentRow.DataBoundItem as DataRowView;

            return view == null ? null : view.Row;
        }

        // ------------------------------------------------------------
        // Loading
        // ------------------------------------------------------------
        // Derived forms put their query here. Filters: DtpFrom, DtpTo, TxtSearch.
        protected virtual void LoadData()
        {
        }

        protected void Reload()
        {
            if (_suspendReload)
            {
                return;
            }

            if (DtpTo.Value.Date < DtpFrom.Value.Date)
            {
                MessageBox.Show("\"To\" cannot be earlier than \"From\".",
                    "Date range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // Lets a derived form change several filters without reloading each time.
        protected void SetFiltersQuietly(Action change)
        {
            _suspendReload = true;

            try
            {
                change();
            }
            finally
            {
                _suspendReload = false;
            }
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            // Not before the form is shown: the Load event does the first load.
            if (IsHandleCreated)
            {
                Reload();
            }
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Reload();
            }
        }

        // ------------------------------------------------------------
        // CSV export (what the grid shows, visible columns only)
        // ------------------------------------------------------------
        protected void ExportCsv()
        {
            if (Grid.Rows.Count == 0)
            {
                MessageBox.Show("There is nothing to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "CSV file (*.csv)|*.csv";
                dlg.FileName = _csvName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";

                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    StringBuilder sb = new StringBuilder();
                    System.Collections.Generic.List<DataGridViewColumn> cols = VisibleColumns();

                    for (int i = 0; i < cols.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(',');
                        }

                        sb.Append(CsvCell(cols[i].HeaderText));
                    }

                    sb.AppendLine();

                    foreach (DataGridViewRow row in Grid.Rows)
                    {
                        for (int i = 0; i < cols.Count; i++)
                        {
                            if (i > 0)
                            {
                                sb.Append(',');
                            }

                            sb.Append(CsvCell(CellText(row.Cells[cols[i].Index].Value)));
                        }

                        sb.AppendLine();
                    }

                    // UTF-8 with BOM so Excel shows the peso sign and accents correctly.
                    File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));

                    MessageBox.Show("Saved " + Grid.Rows.Count + " row(s) to:\n" + dlg.FileName,
                        "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private System.Collections.Generic.List<DataGridViewColumn> VisibleColumns()
        {
            System.Collections.Generic.List<DataGridViewColumn> list =
                new System.Collections.Generic.List<DataGridViewColumn>();

            foreach (DataGridViewColumn c in Grid.Columns)
            {
                if (c.Visible)
                {
                    list.Add(c);
                }
            }

            list.Sort(delegate (DataGridViewColumn a, DataGridViewColumn b)
            {
                return a.DisplayIndex.CompareTo(b.DisplayIndex);
            });

            return list;
        }

        private static string CellText(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "";
            }

            if (value is DateTime)
            {
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }

            if (value is decimal)
            {
                return ((decimal)value).ToString("0.##", CultureInfo.InvariantCulture);
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static string CsvCell(string text)
        {
            if (text == null)
            {
                text = "";
            }

            // A cell that starts with = + - @ would run as a formula in Excel.
            if (text.Length > 0 && "=+-@".IndexOf(text[0]) >= 0)
            {
                double ignore;

                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out ignore))
                {
                    text = "'" + text;
                }
            }

            if (text.IndexOfAny(new char[] { ',', '"', '\r', '\n' }) >= 0)
            {
                text = "\"" + text.Replace("\"", "\"\"") + "\"";
            }

            return text;
        }
    }
}
