//using CinemaBookingSystem.Database;
//using System;
//using System.Data;
//using System.Drawing;
//using System.Windows.Forms;

//namespace CinemaBookingSystem.Forms
//{
//    public partial class ReportsForm : Form
//    {
//        private DatabaseHelper db;
//        private DataGridView dgvReport;
//        private ComboBox cmbReportType;
//        private Button btnLoad;

//        public ReportsForm()
//        {
//            InitializeComponent();
//            db = new DatabaseHelper();
//        }

//        private void InitializeComponent()
//        {
//            this.Size = new Size(1000, 600);
//            this.BackColor = Color.FromArgb(240, 240, 245);

//            Label lblTitle = new Label();
//            lblTitle.Text = "Reports";
//            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
//            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
//            lblTitle.Size = new Size(300, 50);
//            lblTitle.Location = new Point(20, 20);

//            Label lblReportType = new Label();
//            lblReportType.Text = "Report Type:";
//            lblReportType.Font = new Font("Segoe UI", 11);
//            lblReportType.Location = new Point(20, 90);
//            lblReportType.Size = new Size(100, 25);

//            cmbReportType = new ComboBox();
//            cmbReportType.Size = new Size(250, 30);
//            cmbReportType.Location = new Point(130, 88);
//            cmbReportType.DropDownStyle = ComboBoxStyle.DropDownList;
//            cmbReportType.Items.Add("Movie Revenue");
//            cmbReportType.Items.Add("Top Rated Movies");
//            cmbReportType.Items.Add("Today's Shows");
//            cmbReportType.SelectedIndex = 0;

//            btnLoad = new Button();
//            btnLoad.Text = "Load Report";
//            btnLoad.Size = new Size(120, 35);
//            btnLoad.Location = new Point(400, 86);
//            btnLoad.BackColor = Color.FromArgb(52, 152, 219);
//            btnLoad.ForeColor = Color.White;
//            btnLoad.FlatStyle = FlatStyle.Flat;
//            btnLoad.Click += btnLoad_Click;

//            dgvReport = new DataGridView();
//            dgvReport.Location = new Point(20, 140);
//            dgvReport.Size = new Size(950, 420);
//            dgvReport.BackgroundColor = Color.White;
//            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
//            dgvReport.ReadOnly = true;

//            this.Controls.Add(lblTitle);
//            this.Controls.Add(lblReportType);
//            this.Controls.Add(cmbReportType);
//            this.Controls.Add(btnLoad);
//            this.Controls.Add(dgvReport);
//        }

//        private void btnLoad_Click(object sender, EventArgs e)
//        {
//            string query = "";

//            switch (cmbReportType.SelectedIndex)
//            {
//                case 0: // Movie Revenue
//                    query = @"
//                        SELECT m.Title, COUNT(b.BookingID) AS TotalBookings, 
//                               SUM(b.TotalAmount) AS TotalRevenue
//                        FROM Movies m
//                        JOIN Shows s ON m.MovieID = s.MovieID
//                        JOIN Bookings b ON s.ShowID = b.ShowID
//                        WHERE b.Status = 'Booked'
//                        GROUP BY m.MovieID, m.Title
//                        ORDER BY TotalRevenue DESC";
//                    break;
//                case 1: // Top Rated Movies
//                    query = @"
//                        SELECT m.Title, m.Rating, COUNT(r.ReviewID) AS ReviewCount
//                        FROM Movies m
//                        LEFT JOIN Reviews r ON m.MovieID = r.MovieID
//                        GROUP BY m.MovieID, m.Title, m.Rating
//                        ORDER BY m.Rating DESC";
//                    break;
//                case 2: // Today's Shows
//                    query = @"
//                        SELECT m.Title, h.HallName, s.StartTime, s.EndTime, s.TicketPrice
//                        FROM Shows s
//                        JOIN Movies m ON s.MovieID = m.MovieID
//                        JOIN Halls h ON s.HallID = h.HallID
//                        WHERE s.ShowDate = CURDATE()
//                        ORDER BY s.StartTime";
//                    break;
//            }

//            DataTable dt = db.ExecuteQuery(query);
//            dgvReport.DataSource = dt;
//        }
//    }
//}

using CinemaBookingSystem.Database;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using Font = System.Drawing.Font;

namespace CinemaBookingSystem.Forms
{
    public partial class ReportsForm : Form
    {
        private DatabaseHelper db;
        private DataGridView dgvReport;
        private ComboBox cmbReportType;
        private Button btnLoad;
        private Button btnDownloadPDF;

        public ReportsForm()
        {
            InitializeComponent();
            db = new DatabaseHelper();
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(1000, 600);
            this.BackColor = System.Drawing.Color.FromArgb(240, 240, 245);

            Label lblTitle = new Label();
            lblTitle.Text = "📝 Reports";
            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(44, 62, 80);
            lblTitle.Size = new System.Drawing.Size(300, 50);
            lblTitle.Location = new System.Drawing.Point(20, 20);

            Label lblReportType = new Label();
            lblReportType.Text = "Report Type:";
            lblReportType.Font = new Font("Segoe UI", 11);
            lblReportType.Location = new System.Drawing.Point(20, 90);
            lblReportType.Size = new System.Drawing.Size(100, 25);

            cmbReportType = new ComboBox();
            cmbReportType.Size = new System.Drawing.Size(250, 30);
            cmbReportType.Location = new System.Drawing.Point(130, 88);
            cmbReportType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbReportType.Items.Add("Movie Revenue");
            cmbReportType.Items.Add("Top Rated Movies");
            cmbReportType.Items.Add("Today's Shows");
            cmbReportType.SelectedIndex = 0;

            // Load Button
            btnLoad = new Button();
            btnLoad.Text = "Load Report";
            btnLoad.Size = new System.Drawing.Size(120, 35);
            btnLoad.Location = new System.Drawing.Point(400, 86);
            btnLoad.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
            btnLoad.ForeColor = System.Drawing.Color.White;
            btnLoad.FlatStyle = FlatStyle.Flat;
            btnLoad.FlatAppearance.BorderSize = 0;
            btnLoad.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnLoad.Cursor = Cursors.Hand;
            btnLoad.Click += btnLoad_Click;

            // Download PDF Button
            btnDownloadPDF = new Button();
            btnDownloadPDF.Text = "⬇ Download PDF";
            btnDownloadPDF.Size = new System.Drawing.Size(155, 35);
            btnDownloadPDF.Location = new System.Drawing.Point(540, 86);
            btnDownloadPDF.BackColor = System.Drawing.Color.FromArgb(192, 57, 43);
            btnDownloadPDF.ForeColor = System.Drawing.Color.White;
            btnDownloadPDF.FlatStyle = FlatStyle.Flat;
            btnDownloadPDF.FlatAppearance.BorderSize = 0;
            btnDownloadPDF.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnDownloadPDF.Cursor = Cursors.Hand;
            btnDownloadPDF.Enabled = false;
            btnDownloadPDF.Click += btnDownloadPDF_Click;

            // DataGridView
            dgvReport = new DataGridView();
            dgvReport.Location = new System.Drawing.Point(20, 140);
            dgvReport.Size = new System.Drawing.Size(950, 420);
            dgvReport.BackgroundColor = System.Drawing.Color.White;
            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReport.ReadOnly = true;
            dgvReport.BorderStyle = BorderStyle.None;
            dgvReport.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvReport.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvReport.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(44, 62, 80);
            dgvReport.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvReport.EnableHeadersVisualStyles = false;
            dgvReport.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(245, 245, 250);
            dgvReport.RowHeadersVisible = false;

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblReportType);
            this.Controls.Add(cmbReportType);
            this.Controls.Add(btnLoad);
            this.Controls.Add(btnDownloadPDF);
            this.Controls.Add(dgvReport);
        }

        // Load Report
        private void btnLoad_Click(object sender, EventArgs e)
        {
            string query = "";

            switch (cmbReportType.SelectedIndex)
            {
                case 0:
                    query = @"
                        SELECT m.Title, COUNT(b.BookingID) AS TotalBookings, 
                               SUM(b.TotalAmount) AS TotalRevenue
                        FROM Movies m
                        JOIN Shows s ON m.MovieID = s.MovieID
                        JOIN Bookings b ON s.ShowID = b.ShowID
                        WHERE b.Status = 'Booked'
                        GROUP BY m.MovieID, m.Title
                        ORDER BY TotalRevenue DESC";
                    break;
                case 1:
                    query = @"
                        SELECT m.Title, m.Rating, COUNT(r.ReviewID) AS ReviewCount
                        FROM Movies m
                        LEFT JOIN Reviews r ON m.MovieID = r.MovieID
                        GROUP BY m.MovieID, m.Title, m.Rating
                        ORDER BY m.Rating DESC";
                    break;
                case 2:
                    query = @"
                        SELECT m.Title, h.HallName, s.StartTime, s.EndTime, s.TicketPrice
                        FROM Shows s
                        JOIN Movies m ON s.MovieID = m.MovieID
                        JOIN Halls h ON s.HallID = h.HallID
                        WHERE s.ShowDate = CURDATE()
                        ORDER BY s.StartTime";
                    break;
            }

            DataTable dt = db.ExecuteQuery(query);
            dgvReport.DataSource = dt;
            btnDownloadPDF.Enabled = dt.Rows.Count > 0;

            if (dt.Rows.Count == 0)
                MessageBox.Show("No data found for this report.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Download PDF
        private void btnDownloadPDF_Click(object sender, EventArgs e)
        {
            if (dgvReport.DataSource == null || dgvReport.Rows.Count == 0)
            {
                MessageBox.Show("Please load a report first!", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PDF File (*.pdf)|*.pdf";
            saveDialog.FileName = $"{cmbReportType.SelectedItem}_{DateTime.Now:yyyy-MM-dd}";
            saveDialog.Title = "Save PDF Report";

            if (saveDialog.ShowDialog() != DialogResult.OK) return;

            try
            {
                // Page setup - landscape for wide tables
                Document doc = new Document(PageSize.A4.Rotate(), 20f, 20f, 30f, 20f);
                PdfWriter.GetInstance(doc, new FileStream(saveDialog.FileName, FileMode.Create));
                doc.Open();

                // Colors
                BaseColor headerBg = new BaseColor(44, 62, 80);
                BaseColor headerFg = BaseColor.WHITE;
                BaseColor altRowBg = new BaseColor(245, 245, 250);
                BaseColor normalBg = BaseColor.WHITE;
                BaseColor titleColor = new BaseColor(44, 62, 80);

                // Fonts
                iTextSharp.text.Font titleFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 18, iTextSharp.text.Font.BOLD, titleColor);
                iTextSharp.text.Font subFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);
                iTextSharp.text.Font headerFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, headerFg);
                iTextSharp.text.Font cellFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

                // Title
                Paragraph title = new Paragraph($"Cinema Booking System", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                doc.Add(title);

                Paragraph subtitle = new Paragraph($"Report: {cmbReportType.SelectedItem}     Date: {DateTime.Now:dd MMM yyyy}", subFont);
                subtitle.Alignment = Element.ALIGN_CENTER;
                subtitle.SpacingAfter = 15f;
                doc.Add(subtitle);

                // Table
                int colCount = dgvReport.Columns.Count;
                PdfPTable table = new PdfPTable(colCount);
                table.WidthPercentage = 100;

                // Header row
                for (int i = 0; i < colCount; i++)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(dgvReport.Columns[i].HeaderText, headerFont));
                    cell.BackgroundColor = headerBg;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 8f;
                    cell.BorderColor = BaseColor.WHITE;
                    table.AddCell(cell);
                }

                // Data rows
                int rowIndex = 0;
                foreach (DataGridViewRow row in dgvReport.Rows)
                {
                    BaseColor bg = (rowIndex % 2 == 0) ? normalBg : altRowBg;
                    for (int i = 0; i < colCount; i++)
                    {
                        string val = row.Cells[i].Value?.ToString() ?? "";
                        PdfPCell cell = new PdfPCell(new Phrase(val, cellFont));
                        cell.BackgroundColor = bg;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.Padding = 6f;
                        cell.BorderColor = new BaseColor(220, 220, 220);
                        table.AddCell(cell);
                    }
                    rowIndex++;
                }

                doc.Add(table);

                // Footer
                Paragraph footer = new Paragraph($"\nTotal Records: {dgvReport.Rows.Count}     Generated: {DateTime.Now:dd MMM yyyy hh:mm tt}", subFont);
                footer.Alignment = Element.ALIGN_RIGHT;
                footer.SpacingBefore = 10f;
                doc.Add(footer);

                doc.Close();

                MessageBox.Show($"✅ PDF report saved successfully!\nLocation: {saveDialog.FileName}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // PDF seedha kholo
                System.Diagnostics.Process.Start(saveDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error creating PDF: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}