using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using CinemaBookingSystem.Database;
using CinemaBookingSystem.Utils;

namespace CinemaBookingSystem.Forms
{
    public partial class BookingForm : Form
    {
        private DatabaseHelper db;
        private ComboBox cmbMovie;
        private ComboBox cmbShow;
        private Panel pnlSeats;
        private Button btnConfirmBooking;
        private Label lblTotalAmount;
        private ComboBox cmbPaymentMethod;
        private List<int> selectedSeats;
        private int selectedShowID;
        private decimal ticketPrice;
        private Label lblSelectedSeatsInfo;
        private bool isLoading = true; // Flag to prevent popup on form load

        public BookingForm()
        {
            InitializeComponent();
            db = new DatabaseHelper();
            selectedSeats = new List<int>();
            isLoading = true;
            LoadMovies();
            LoadPaymentMethods();
            isLoading = false;
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 750);
            this.BackColor = Color.FromArgb(240, 240, 245);
            this.AutoScroll = true;

            Label lblTitle = new Label();
            lblTitle.Text = "Book Tickets";
            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Size = new Size(300, 50);
            lblTitle.Location = new Point(20, 20);

            // Movie Selection
            Label lblMovie = new Label();
            lblMovie.Text = "Select Movie:";
            lblMovie.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblMovie.Location = new Point(30, 90);
            lblMovie.Size = new Size(120, 30);

            cmbMovie = new ComboBox();
            cmbMovie.Size = new Size(300, 35);
            cmbMovie.Location = new Point(160, 88);
            cmbMovie.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMovie.Font = new Font("Segoe UI", 11);
            cmbMovie.SelectedIndexChanged += cmbMovie_SelectedIndexChanged;

            // Show Selection
            Label lblShow = new Label();
            lblShow.Text = "Select Show:";
            lblShow.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblShow.Location = new Point(30, 135);
            lblShow.Size = new Size(120, 30);

            cmbShow = new ComboBox();
            cmbShow.Size = new Size(400, 35);
            cmbShow.Location = new Point(160, 133);
            cmbShow.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbShow.Font = new Font("Segoe UI", 10);
            cmbShow.SelectedIndexChanged += cmbShow_SelectedIndexChanged;

            // Seats Panel
            Label lblSeats = new Label();
            lblSeats.Text = "Select Seats:";
            lblSeats.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblSeats.Location = new Point(30, 185);
            lblSeats.Size = new Size(120, 30);

            pnlSeats = new Panel();
            pnlSeats.Size = new Size(850, 350);
            pnlSeats.Location = new Point(30, 220);
            pnlSeats.BackColor = Color.White;
            pnlSeats.BorderStyle = BorderStyle.FixedSingle;
            pnlSeats.AutoScroll = true;
            pnlSeats.BackColor = Color.FromArgb(245, 245, 250);

            // Selected Seats Info
            lblSelectedSeatsInfo = new Label();
            lblSelectedSeatsInfo.Text = "Selected Seats: None";
            lblSelectedSeatsInfo.Font = new Font("Segoe UI", 10);
            lblSelectedSeatsInfo.ForeColor = Color.FromArgb(52, 73, 94);
            lblSelectedSeatsInfo.Size = new Size(300, 25);
            lblSelectedSeatsInfo.Location = new Point(30, 585);

            // Total Amount
            Label lblTotal = new Label();
            lblTotal.Text = "Total Amount:";
            lblTotal.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTotal.Location = new Point(30, 620);
            lblTotal.Size = new Size(130, 35);

            lblTotalAmount = new Label();
            lblTotalAmount.Text = "Rs. 0";
            lblTotalAmount.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTotalAmount.ForeColor = Color.FromArgb(46, 204, 113);
            lblTotalAmount.Location = new Point(170, 618);
            lblTotalAmount.Size = new Size(200, 35);

            // Payment Method
            Label lblPayment = new Label();
            lblPayment.Text = "Payment Method:";
            lblPayment.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblPayment.Location = new Point(30, 665);
            lblPayment.Size = new Size(130, 30);

            cmbPaymentMethod = new ComboBox();
            cmbPaymentMethod.Size = new Size(200, 35);
            cmbPaymentMethod.Location = new Point(170, 663);
            cmbPaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaymentMethod.Font = new Font("Segoe UI", 11);

            // Confirm Button
            btnConfirmBooking = new Button();
            btnConfirmBooking.Text = "Confirm Booking";
            btnConfirmBooking.Size = new Size(180, 45);
            btnConfirmBooking.Location = new Point(700, 655);
            btnConfirmBooking.BackColor = Color.FromArgb(46, 204, 113);
            btnConfirmBooking.ForeColor = Color.White;
            btnConfirmBooking.FlatStyle = FlatStyle.Flat;
            btnConfirmBooking.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnConfirmBooking.Click += btnConfirmBooking_Click;

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblMovie);
            this.Controls.Add(cmbMovie);
            this.Controls.Add(lblShow);
            this.Controls.Add(cmbShow);
            this.Controls.Add(lblSeats);
            this.Controls.Add(pnlSeats);
            this.Controls.Add(lblSelectedSeatsInfo);
            this.Controls.Add(lblTotal);
            this.Controls.Add(lblTotalAmount);
            this.Controls.Add(lblPayment);
            this.Controls.Add(cmbPaymentMethod);
            this.Controls.Add(btnConfirmBooking);
        }

        private void LoadPaymentMethods()
        {
            try
            {
                string query = "SELECT MethodID, MethodName FROM PaymentMethods";
                DataTable dt = db.ExecuteQuery(query);
                if (dt.Rows.Count > 0)
                {
                    cmbPaymentMethod.DisplayMember = "MethodName";
                    cmbPaymentMethod.ValueMember = "MethodID";
                    cmbPaymentMethod.DataSource = dt;
                }
                else
                {
                    cmbPaymentMethod.Items.Add(new { MethodID = 1, MethodName = "Cash" });
                    cmbPaymentMethod.Items.Add(new { MethodID = 2, MethodName = "Credit Card" });
                    cmbPaymentMethod.DisplayMember = "MethodName";
                    cmbPaymentMethod.ValueMember = "MethodID";
                    cmbPaymentMethod.SelectedIndex = 0;
                }
            }
            catch
            {
                cmbPaymentMethod.Items.Add(new { MethodID = 1, MethodName = "Cash" });
                cmbPaymentMethod.Items.Add(new { MethodID = 2, MethodName = "Credit Card" });
                cmbPaymentMethod.DisplayMember = "MethodName";
                cmbPaymentMethod.ValueMember = "MethodID";
                cmbPaymentMethod.SelectedIndex = 0;
            }
        }

        private void LoadMovies()
        {
            try
            {
                string query = "SELECT MovieID, Title FROM Movies ORDER BY Title";
                DataTable dt = db.ExecuteQuery(query);
                cmbMovie.DisplayMember = "Title";
                cmbMovie.ValueMember = "MovieID";
                cmbMovie.DataSource = dt;

                if (cmbMovie.Items.Count > 0)
                    cmbMovie.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading movies: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbMovie_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Form load hone par popup nahi aayega
            if (isLoading) return;

            if (cmbMovie.SelectedValue != null && int.TryParse(cmbMovie.SelectedValue.ToString(), out int movieID))
            {
                LoadShows(movieID);
            }
        }

        private void LoadShows(int movieID)
        {
            try
            {
                string query = @"
                    SELECT s.ShowID, s.ShowDate, s.StartTime, s.EndTime, h.HallName,
                           s.TicketPrice,
                           (SELECT COUNT(*) FROM Seats WHERE HallID = s.HallID AND Status = 'Available') AS AvailableSeats
                    FROM Shows s
                    JOIN Halls h ON s.HallID = h.HallID
                    WHERE s.MovieID = @movieID AND s.ShowDate >= CURDATE()
                    ORDER BY s.ShowDate, s.StartTime";

                MySqlParameter[] parameters = { new MySqlParameter("@movieID", movieID) };
                DataTable dt = db.ExecuteQuery(query, parameters);

                cmbShow.Items.Clear();
                pnlSeats.Controls.Clear();

                if (dt.Rows.Count == 0)
                {
                    // Sirf dropdown mein message dikhao — koi popup nahi
                    cmbShow.Items.Add("--no show is available--");
                    cmbShow.SelectedIndex = 0;
                    cmbShow.Enabled = false;

                    // Seats panel mein bhi message
                    Label lblNoShow = new Label();
                    lblNoShow.Text = "⚠ No upcoming show is available for this movie.\nContact to Admin.";
                    lblNoShow.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                    lblNoShow.ForeColor = Color.FromArgb(231, 76, 60);
                    lblNoShow.Location = new Point(20, 50);
                    lblNoShow.Size = new Size(600, 60);
                    lblNoShow.TextAlign = ContentAlignment.MiddleLeft;
                    pnlSeats.Controls.Add(lblNoShow);
                    return;
                }

                cmbShow.Enabled = true;
                foreach (DataRow row in dt.Rows)
                {
                    var item = new
                    {
                        ShowID = Convert.ToInt32(row["ShowID"]),
                        TicketPrice = Convert.ToDecimal(row["TicketPrice"]),
                        ShowDate = Convert.ToDateTime(row["ShowDate"]),
                        StartTime = row["StartTime"].ToString(),
                        EndTime = row["EndTime"].ToString(),
                        HallName = row["HallName"].ToString(),
                        AvailableSeats = Convert.ToInt32(row["AvailableSeats"]),
                        DisplayText = $"{Convert.ToDateTime(row["ShowDate"]):dd-MMM-yyyy} | {row["StartTime"]} - {row["EndTime"]} | {row["HallName"]} | {row["AvailableSeats"]} seats left | Rs.{row["TicketPrice"]}/seat"
                    };
                    cmbShow.Items.Add(item);
                }
                cmbShow.DisplayMember = "DisplayText";
                if (cmbShow.Items.Count > 0)
                    cmbShow.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading shows: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbShow_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isLoading) return;
            if (cmbShow.SelectedItem != null && cmbShow.Enabled)
            {
                dynamic selected = cmbShow.SelectedItem;
                try
                {
                    selectedShowID = selected.ShowID;
                    ticketPrice = selected.TicketPrice;
                    LoadSeats(selectedShowID);
                }
                catch { }
            }
        }

        private void LoadSeats(int showID)
        {
            pnlSeats.Controls.Clear();
            selectedSeats.Clear();
            UpdateTotal();
            UpdateSelectedSeatsInfo();

            try
            {
                string query = @"
                    SELECT s.SeatID, s.SeatNumber, st.TypeName, st.ExtraPrice, 
                           sh.TicketPrice, sh.TicketPrice + st.ExtraPrice AS TotalPrice
                    FROM Shows sh
                    JOIN Seats s ON s.HallID = sh.HallID
                    JOIN SeatTypes st ON s.SeatTypeID = st.SeatTypeID
                    WHERE sh.ShowID = @showID AND s.Status = 'Available'
                    ORDER BY s.SeatNumber";

                MySqlParameter[] parameters = { new MySqlParameter("@showID", showID) };
                DataTable dt = db.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    Label lblScreen = new Label();
                    lblScreen.Text = "=== S C R E E N ===";
                    lblScreen.Font = new Font("Segoe UI", 14, FontStyle.Bold);
                    lblScreen.ForeColor = Color.White;
                    lblScreen.BackColor = Color.FromArgb(44, 62, 80);
                    lblScreen.Size = new Size(300, 40);
                    lblScreen.Location = new Point(pnlSeats.Width / 2 - 150, 15);
                    lblScreen.TextAlign = ContentAlignment.MiddleCenter;
                    pnlSeats.Controls.Add(lblScreen);

                    int x = 30, y = 70;
                    int seatWidth = 85, seatHeight = 80;
                    int cols = 8;
                    int rowCount = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        int seatID = Convert.ToInt32(row["SeatID"]);
                        string seatNumber = row["SeatNumber"].ToString();
                        string seatType = row["TypeName"].ToString();
                        decimal totalPrice = Convert.ToDecimal(row["TotalPrice"]);

                        Button btnSeat = new Button();
                        btnSeat.Text = $"{seatNumber}\n{seatType}\nRs.{totalPrice:N0}";
                        btnSeat.Size = new Size(seatWidth, seatHeight);
                        btnSeat.Location = new Point(x, y);
                        btnSeat.BackColor = Color.FromArgb(52, 152, 219);
                        btnSeat.ForeColor = Color.White;
                        btnSeat.Font = new Font("Segoe UI", 8, FontStyle.Bold);
                        btnSeat.Tag = seatID;
                        btnSeat.FlatStyle = FlatStyle.Flat;
                        btnSeat.Click += (s, ev) => ToggleSeatSelection((Button)s, seatID, totalPrice);

                        ToolTip tooltip = new ToolTip();
                        tooltip.SetToolTip(btnSeat, $"{seatNumber} - {seatType} Seat\nPrice: Rs.{totalPrice:N0}");

                        pnlSeats.Controls.Add(btnSeat);

                        x += seatWidth + 10;
                        rowCount++;

                        if (rowCount >= cols)
                        {
                            x = 30;
                            y += seatHeight + 10;
                            rowCount = 0;
                        }
                    }
                }
                else
                {
                    Label lblNoSeats = new Label();
                    lblNoSeats.Text = "❌ No available seats for this show.\nPlease select another show.";
                    lblNoSeats.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                    lblNoSeats.ForeColor = Color.Red;
                    lblNoSeats.Location = new Point(20, 50);
                    lblNoSeats.Size = new Size(400, 60);
                    lblNoSeats.TextAlign = ContentAlignment.MiddleLeft;
                    pnlSeats.Controls.Add(lblNoSeats);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading seats: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Label lblError = new Label();
                lblError.Text = $"Error loading seats: {ex.Message}";
                lblError.ForeColor = Color.Red;
                lblError.Location = new Point(20, 50);
                lblError.Size = new Size(500, 50);
                pnlSeats.Controls.Add(lblError);
            }
        }

        private void ToggleSeatSelection(Button seatButton, int seatID, decimal price)
        {
            if (seatButton.BackColor == Color.FromArgb(52, 152, 219))
            {
                seatButton.BackColor = Color.FromArgb(46, 204, 113);
                seatButton.Text = seatButton.Text.Replace("Available", "Selected");
                selectedSeats.Add(seatID);
            }
            else
            {
                seatButton.BackColor = Color.FromArgb(52, 152, 219);
                selectedSeats.Remove(seatID);
            }
            UpdateTotal();
            UpdateSelectedSeatsInfo();
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (int seatID in selectedSeats)
            {
                foreach (Control ctrl in pnlSeats.Controls)
                {
                    if (ctrl is Button btn && btn.Tag != null && (int)btn.Tag == seatID)
                    {
                        string text = btn.Text;
                        int lastRs = text.LastIndexOf("Rs.");
                        if (lastRs >= 0)
                        {
                            string priceStr = text.Substring(lastRs + 3).Trim();
                            if (decimal.TryParse(priceStr, out decimal p))
                                total += p;
                        }
                        break;
                    }
                }
            }
            lblTotalAmount.Text = $"Rs. {total:N0}";
        }

        private void UpdateSelectedSeatsInfo()
        {
            if (selectedSeats.Count == 0)
            {
                lblSelectedSeatsInfo.Text = "Selected Seats: None";
                lblSelectedSeatsInfo.ForeColor = Color.FromArgb(52, 73, 94);
            }
            else
            {
                List<string> seatNumbers = new List<string>();
                foreach (int seatID in selectedSeats)
                {
                    foreach (Control ctrl in pnlSeats.Controls)
                    {
                        if (ctrl is Button btn && btn.Tag != null && (int)btn.Tag == seatID)
                        {
                            string text = btn.Text;
                            string seatNum = text.Split('\n')[0];
                            seatNumbers.Add(seatNum);
                            break;
                        }
                    }
                }
                lblSelectedSeatsInfo.Text = $"Selected Seats: {string.Join(", ", seatNumbers)}";
                lblSelectedSeatsInfo.ForeColor = Color.FromArgb(46, 204, 113);
            }
        }

        private void btnConfirmBooking_Click(object sender, EventArgs e)
        {
            if (selectedSeats.Count == 0)
            {
                MessageBox.Show("⚠️ Please select at least one seat!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbPaymentMethod.SelectedValue == null)
            {
                MessageBox.Show("⚠️ Please select a payment method!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Session.CurrentUser?.CustomerID == null)
            {
                MessageBox.Show("⚠️ Please login as customer to book tickets!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int successCount = 0;
            string errorMessage = "";

            foreach (int seatID in selectedSeats)
            {
                try
                {
                    using (MySqlConnection conn = db.GetConnection())
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_book_tickets", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_CustomerID", Session.CurrentUser.CustomerID);
                            cmd.Parameters.AddWithValue("@p_ShowID", selectedShowID);
                            cmd.Parameters.AddWithValue("@p_SeatID", seatID);
                            cmd.Parameters.AddWithValue("@p_MethodID", Convert.ToInt32(cmbPaymentMethod.SelectedValue));

                            MySqlParameter outputBooking = new MySqlParameter("@p_BookingID", MySqlDbType.Int32);
                            outputBooking.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(outputBooking);

                            MySqlParameter outputMessage = new MySqlParameter("@p_Message", MySqlDbType.VarChar, 255);
                            outputMessage.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(outputMessage);

                            cmd.ExecuteNonQuery();

                            if (Convert.ToInt32(outputBooking.Value) > 0)
                                successCount++;
                            else
                                errorMessage = outputMessage.Value.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            }

            if (successCount > 0)
            {
                MessageBox.Show($"✅ Successfully booked {successCount} seat(s)!\nTotal: {lblTotalAmount.Text}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadSeats(selectedShowID);
                selectedSeats.Clear();
                UpdateTotal();
                UpdateSelectedSeatsInfo();
            }
            else
            {
                MessageBox.Show($"❌ Booking failed: {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}