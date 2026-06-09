// Forms/DashboardForm.cs
using CinemaBookingSystem.Database;
using CinemaBookingSystem.Database;
using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;

namespace CinemaBookingSystem.Forms
{
    public partial class DashboardForm : Form
    {
        private DatabaseHelper db;
        private Label lblTotalMovies;
        private Label lblTotalBookings;
        private Label lblTotalRevenue;
        private Label lblAvailableShows;

        public DashboardForm()
        {
            InitializeComponent();
            db = new DatabaseHelper();
            LoadStatistics();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(900, 500);
            this.BackColor = Color.FromArgb(240, 240, 245);

            Label lblTitle = new Label();
            lblTitle.Text = "Dashboard";
            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Size = new Size(300, 50);
            lblTitle.Location = new Point(20, 20);

            // Stats Panel
            Panel statsPanel = new Panel();
            statsPanel.Size = new Size(850, 200);
            statsPanel.Location = new Point(20, 90);
            statsPanel.BackColor = Color.White;

            // Total Movies Card
            Panel movieCard = CreateStatCard("Total Movies", "0", Color.FromArgb(52, 152, 219), 20, 20);
            lblTotalMovies = movieCard.Controls[1] as Label;

            // Total Bookings Card
            Panel bookingCard = CreateStatCard("Total Bookings", "0", Color.FromArgb(46, 204, 113), 220, 20);
            lblTotalBookings = bookingCard.Controls[1] as Label;

            // Total Revenue Card
            Panel revenueCard = CreateStatCard("Total Revenue", "Rs. 0", Color.FromArgb(155, 89, 182), 420, 20);
            lblTotalRevenue = revenueCard.Controls[1] as Label;

            // Available Shows Card
            Panel showsCard = CreateStatCard("Today's Shows", "0", Color.FromArgb(230, 126, 34), 620, 20);
            lblAvailableShows = showsCard.Controls[1] as Label;

            statsPanel.Controls.Add(movieCard);
            statsPanel.Controls.Add(bookingCard);
            statsPanel.Controls.Add(revenueCard);
            statsPanel.Controls.Add(showsCard);

            this.Controls.Add(lblTitle);
            this.Controls.Add(statsPanel);
        }

        private Panel CreateStatCard(string title, string value, Color color, int x, int y)
        {
            Panel card = new Panel();
            card.Size = new Size(190, 100);
            card.Location = new Point(x, y);
            card.BackColor = color;
            card.BorderStyle = BorderStyle.FixedSingle;

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.ForeColor = Color.White;
            lblTitle.Font = new Font("Segoe UI", 10);
            lblTitle.Size = new Size(180, 25);
            lblTitle.Location = new Point(5, 10);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            Label lblValue = new Label();
            lblValue.Text = value;
            lblValue.ForeColor = Color.White;
            lblValue.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblValue.Size = new Size(180, 40);
            lblValue.Location = new Point(5, 40);
            lblValue.TextAlign = ContentAlignment.MiddleCenter;

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);

            return card;
        }

        private void LoadStatistics()
        {
            try
            {
                // Total Movies
                string movieQuery = "SELECT COUNT(*) FROM Movies";
                lblTotalMovies.Text = db.ExecuteScalar(movieQuery)?.ToString() ?? "0";

                // Total Bookings
                string bookingQuery = "SELECT COUNT(*) FROM Bookings WHERE Status = 'Booked'";
                lblTotalBookings.Text = db.ExecuteScalar(bookingQuery)?.ToString() ?? "0";

                // Total Revenue
                string revenueQuery = "SELECT SUM(TotalAmount) FROM Bookings WHERE Status = 'Booked'";
                object revenue = db.ExecuteScalar(revenueQuery);
                lblTotalRevenue.Text = revenue != DBNull.Value ? $"Rs. {Convert.ToDecimal(revenue):N0}" : "Rs. 0";

                // Today's Shows
                string showsQuery = "SELECT COUNT(*) FROM Shows WHERE ShowDate = CURDATE()";
                lblAvailableShows.Text = db.ExecuteScalar(showsQuery)?.ToString() ?? "0";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading statistics: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}