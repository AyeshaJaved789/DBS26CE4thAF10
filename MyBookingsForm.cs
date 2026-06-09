using CinemaBookingSystem.Database;
using CinemaBookingSystem.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBookingSystem.Forms
{
    public partial class MyBookingsForm : Form
    {
        private DatabaseHelper db;
        private DataGridView dgvBookings;
        private ComboBox cmbStatusFilter;
        private Button btnRefresh;
        private Button btnCancelSelected;

        public MyBookingsForm()
        {
            InitializeComponent();
            db = new DatabaseHelper();
            LoadBookings();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 600);
            this.BackColor = Color.FromArgb(240, 240, 245);

            // ── Title — Admin ya User ke hisaab se ────────────
            string role = Session.CurrentUser?.RoleName ?? "User";
            bool isAdmin = role == "Admin";
            bool isStaff = role == "Staff";

            Label lblTitle = new Label();
            lblTitle.Text = (isAdmin || isStaff) ? "All Bookings" : "My Bookings";
            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Size = new Size(400, 50);
            lblTitle.Location = new Point(20, 20);

            // ── Status Filter ──────────────────────────────────
            Label lblFilter = new Label();
            lblFilter.Text = "Filter by Status:";
            lblFilter.Font = new Font("Segoe UI", 10);
            lblFilter.Location = new Point(20, 80);
            lblFilter.Size = new Size(110, 25);

            cmbStatusFilter = new ComboBox();
            cmbStatusFilter.Size = new Size(150, 30);
            cmbStatusFilter.Location = new Point(135, 78);
            cmbStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatusFilter.Items.Add("All");
            cmbStatusFilter.Items.Add("Booked");
            cmbStatusFilter.Items.Add("Cancelled");
            cmbStatusFilter.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndexChanged += (s, e) => LoadBookings();

            // ── Refresh Button ─────────────────────────────────
            btnRefresh = new Button();
            btnRefresh.Text = "🔄 Refresh";
            btnRefresh.Size = new Size(110, 35);
            btnRefresh.Location = new Point(300, 76);
            btnRefresh.BackColor = Color.FromArgb(52, 152, 219);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Click += (s, e) => LoadBookings();

            // ── Cancel Button (User only) ──────────────────────
            btnCancelSelected = new Button();
            btnCancelSelected.Text = "❌ Cancel Selected";
            btnCancelSelected.Size = new Size(150, 35);
            btnCancelSelected.Location = new Point(425, 76);
            btnCancelSelected.BackColor = Color.FromArgb(231, 76, 60);
            btnCancelSelected.ForeColor = Color.White;
            btnCancelSelected.FlatStyle = FlatStyle.Flat;
            btnCancelSelected.FlatAppearance.BorderSize = 0;
            btnCancelSelected.Cursor = Cursors.Hand;
            btnCancelSelected.Visible = !isAdmin; // Admin ke liye cancel button nahi
            btnCancelSelected.Click += BtnCancelSelected_Click;

            // ── DataGridView ───────────────────────────────────
            dgvBookings = new DataGridView();
            dgvBookings.Location = new Point(20, 120);
            dgvBookings.Size = new Size(950, 440);
            dgvBookings.BackgroundColor = Color.White;
            dgvBookings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBookings.ReadOnly = true;
            dgvBookings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBookings.AllowUserToAddRows = false;
            dgvBookings.RowHeadersVisible = false;
            dgvBookings.BorderStyle = BorderStyle.None;
            dgvBookings.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvBookings.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvBookings.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 62, 80);
            dgvBookings.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvBookings.EnableHeadersVisualStyles = false;
            dgvBookings.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 250);
            dgvBookings.CellFormatting += DgvBookings_CellFormatting;

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblFilter);
            this.Controls.Add(cmbStatusFilter);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(btnCancelSelected);
            this.Controls.Add(dgvBookings);
        }

        // ── Status Colors ──────────────────────────────────────
        private void DgvBookings_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvBookings.Columns.Count > 0 &&
                dgvBookings.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "Cancelled")
                {
                    e.CellStyle.BackColor = Color.FromArgb(231, 76, 60);
                    e.CellStyle.ForeColor = Color.White;
                }
                else if (status == "Booked")
                {
                    e.CellStyle.BackColor = Color.FromArgb(46, 204, 113);
                    e.CellStyle.ForeColor = Color.White;
                }
            }
        }

        // ── Load Bookings ──────────────────────────────────────
        private void LoadBookings()
        {
            string role = Session.CurrentUser?.RoleName ?? "User";
            bool isAdmin = role == "Admin";
            bool isStaff = role == "Staff";
            string statusFilter = cmbStatusFilter.SelectedItem?.ToString() ?? "All";

            try
            {
                DataTable dt;
                string statusCondition = statusFilter == "All" ? "" : "AND b.Status = @status";

                if (isAdmin || isStaff)
                {
                    // ── Admin/Staff: SAB ki bookings dikhao ───
                    string query = $@"
                        SELECT b.BookingID, c.FullName AS CustomerName,
                               m.Title AS Movie, h.HallName AS Hall,
                               DATE_FORMAT(s.ShowDate, '%d-%b-%Y') AS ShowDate,
                               TIME_FORMAT(s.StartTime, '%h:%i %p') AS StartTime,
                               b.TotalAmount, b.Status,
                               DATE_FORMAT(b.BookingDate, '%d-%b-%Y %h:%i %p') AS BookedOn
                        FROM Bookings b
                        JOIN Shows s ON b.ShowID = s.ShowID
                        JOIN Movies m ON s.MovieID = m.MovieID
                        JOIN Halls h ON s.HallID = h.HallID
                        JOIN Customers c ON b.CustomerID = c.CustomerID
                        WHERE 1=1 {statusCondition}
                        ORDER BY b.BookingDate DESC";

                    MySqlParameter[] p = statusFilter == "All"
                        ? new MySqlParameter[0]
                        : new[] { new MySqlParameter("@status", statusFilter) };

                    dt = db.ExecuteQuery(query, p);
                }
                else
                {
                    // ── User: Sirf apni bookings ───────────────
                    if (Session.CurrentUser?.CustomerID == null)
                    {
                        MessageBox.Show("There is a need of customer account for check booking!",
                            "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    string query = $@"
                        SELECT b.BookingID, m.Title AS Movie, h.HallName AS Hall,
                               DATE_FORMAT(s.ShowDate, '%d-%b-%Y') AS ShowDate,
                               TIME_FORMAT(s.StartTime, '%h:%i %p') AS StartTime,
                               TIME_FORMAT(s.EndTime, '%h:%i %p') AS EndTime,
                               b.TotalAmount, b.Status,
                               DATE_FORMAT(b.BookingDate, '%d-%b-%Y %h:%i %p') AS BookedOn
                        FROM Bookings b
                        JOIN Shows s ON b.ShowID = s.ShowID
                        JOIN Movies m ON s.MovieID = m.MovieID
                        JOIN Halls h ON s.HallID = h.HallID
                        WHERE b.CustomerID = @customerID {statusCondition}
                        ORDER BY b.BookingDate DESC";

                    MySqlParameter[] p = statusFilter == "All"
                        ? new[] { new MySqlParameter("@customerID", Session.CurrentUser.CustomerID) }
                        : new[] {
                            new MySqlParameter("@customerID", Session.CurrentUser.CustomerID),
                            new MySqlParameter("@status", statusFilter)
                          };

                    dt = db.ExecuteQuery(query, p);
                }

                dgvBookings.DataSource = dt;
                FormatColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bookings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Column Formatting ──────────────────────────────────
        private void FormatColumns()
        {
            if (dgvBookings.Columns.Contains("BookingID"))
                dgvBookings.Columns["BookingID"].HeaderText = "ID";
            if (dgvBookings.Columns.Contains("CustomerName"))
                dgvBookings.Columns["CustomerName"].HeaderText = "Customer";
            if (dgvBookings.Columns.Contains("Movie"))
                dgvBookings.Columns["Movie"].HeaderText = "Movie";
            if (dgvBookings.Columns.Contains("Hall"))
                dgvBookings.Columns["Hall"].HeaderText = "Hall";
            if (dgvBookings.Columns.Contains("ShowDate"))
                dgvBookings.Columns["ShowDate"].HeaderText = "Show Date";
            if (dgvBookings.Columns.Contains("StartTime"))
                dgvBookings.Columns["StartTime"].HeaderText = "Time";
            if (dgvBookings.Columns.Contains("TotalAmount"))
            {
                dgvBookings.Columns["TotalAmount"].HeaderText = "Total (Rs.)";
                dgvBookings.Columns["TotalAmount"].DefaultCellStyle.Format = "N2";
            }
            if (dgvBookings.Columns.Contains("Status"))
                dgvBookings.Columns["Status"].HeaderText = "Status";
            if (dgvBookings.Columns.Contains("BookedOn"))
                dgvBookings.Columns["BookedOn"].HeaderText = "Booked On";
        }

        // ── Cancel Booking (User only) ─────────────────────────
        private void BtnCancelSelected_Click(object sender, EventArgs e)
        {
            if (dgvBookings.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pehle koi booking select karein!", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int bookingID = Convert.ToInt32(dgvBookings.SelectedRows[0].Cells["BookingID"].Value);
            string status = dgvBookings.SelectedRows[0].Cells["Status"].Value.ToString();

            if (status == "Cancelled")
            {
                MessageBox.Show("This booking is already cancelled!", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Do you want to cancel the booking according to cancellation policy",
                "Confirm Cancellation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection conn = db.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_cancel_booking", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_BookingID", bookingID);

                            MySqlParameter refundParam = new MySqlParameter("@p_Refund", MySqlDbType.Decimal);
                            refundParam.Direction = ParameterDirection.Output;
                            refundParam.Precision = 10;
                            refundParam.Scale = 2;
                            cmd.Parameters.Add(refundParam);

                            MySqlParameter messageParam = new MySqlParameter("@p_Message", MySqlDbType.VarChar, 255);
                            messageParam.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(messageParam);

                            cmd.ExecuteNonQuery();

                            decimal refund = Convert.ToDecimal(refundParam.Value);
                            string message = messageParam.Value.ToString();

                            MessageBox.Show($"{message}\nRefund: Rs. {refund:N2}",
                                "Cancellation Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadBookings();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}