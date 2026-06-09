using CinemaBookingSystem.Database;
using CinemaBookingSystem.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBookingSystem.Forms
{
    public partial class ReviewsForm : Form
    {
        private DatabaseHelper db;
        private ComboBox cmbMovie;
        private NumericUpDown numRating;
        private TextBox txtComment;
        private Button btnSubmit;
        private DataGridView dgvReviews;

        public ReviewsForm()
        {
            InitializeComponent();
            db = new DatabaseHelper();
            LoadMovies();
            LoadReviews();
        }

        private void InitializeComponent()
        {
            // Form Properties
            this.Text = "Movie Reviews";
            this.Size = new Size(1100, 600);
            this.BackColor = Color.FromArgb(240, 240, 245);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Title Label
            Label lblTitle = new Label();
            lblTitle.Text = "Movie Reviews";
            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Location = new Point(20, 20);
            lblTitle.Size = new Size(300, 50);

            // ==================== LEFT SIDE - ADD REVIEW ====================
            GroupBox grpAddReview = new GroupBox();
            grpAddReview.Text = "Write a Review";
            grpAddReview.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            grpAddReview.Location = new Point(20, 80);
            grpAddReview.Size = new Size(450, 300);
            grpAddReview.BackColor = Color.White;

            // Movie Label
            Label lblMovie = new Label();
            lblMovie.Text = "Movie:";
            lblMovie.Font = new Font("Segoe UI", 10);
            lblMovie.Location = new Point(20, 35);
            lblMovie.Size = new Size(80, 25);

            // Movie ComboBox
            cmbMovie = new ComboBox();
            cmbMovie.Location = new Point(110, 33);
            cmbMovie.Size = new Size(300, 28);
            cmbMovie.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMovie.Font = new Font("Segoe UI", 10);

            // Rating Label
            Label lblRating = new Label();
            lblRating.Text = "Rating (1-5):";
            lblRating.Font = new Font("Segoe UI", 10);
            lblRating.Location = new Point(20, 75);
            lblRating.Size = new Size(100, 25);

            // Rating NumericUpDown
            numRating = new NumericUpDown();
            numRating.Location = new Point(130, 73);
            numRating.Size = new Size(60, 27);
            numRating.Minimum = 1;
            numRating.Maximum = 5;
            numRating.Value = 5;

            // Comment Label
            Label lblComment = new Label();
            lblComment.Text = "Comment:";
            lblComment.Font = new Font("Segoe UI", 10);
            lblComment.Location = new Point(20, 115);
            lblComment.Size = new Size(80, 25);

            // Comment TextBox
            txtComment = new TextBox();
            txtComment.Location = new Point(20, 145);
            txtComment.Size = new Size(410, 100);
            txtComment.Multiline = true;
            txtComment.ScrollBars = ScrollBars.Vertical;
            txtComment.Font = new Font("Segoe UI", 10);

            // Submit Button - YAHAN BUTTON HAI
            btnSubmit = new Button();
            btnSubmit.Text = "Submit Review";
            btnSubmit.Location = new Point(270, 260);
            btnSubmit.Size = new Size(160, 35);
            btnSubmit.BackColor = Color.FromArgb(46, 204, 113);
            btnSubmit.ForeColor = Color.White;
            btnSubmit.FlatStyle = FlatStyle.Flat;
            btnSubmit.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnSubmit.Click += BtnSubmit_Click;

            // Add controls to GroupBox
            grpAddReview.Controls.Add(lblMovie);
            grpAddReview.Controls.Add(cmbMovie);
            grpAddReview.Controls.Add(lblRating);
            grpAddReview.Controls.Add(numRating);
            grpAddReview.Controls.Add(lblComment);
            grpAddReview.Controls.Add(txtComment);
            grpAddReview.Controls.Add(btnSubmit);

            // ==================== RIGHT SIDE - REVIEWS LIST ====================
            Label lblReviewsList = new Label();
            lblReviewsList.Text = "All Reviews";
            lblReviewsList.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblReviewsList.ForeColor = Color.FromArgb(44, 62, 80);
            lblReviewsList.Location = new Point(500, 65);
            lblReviewsList.Size = new Size(200, 35);

            // DataGridView
            dgvReviews = new DataGridView();
            dgvReviews.Location = new Point(500, 110);
            dgvReviews.Size = new Size(560, 420);
            dgvReviews.BackgroundColor = Color.White;
            dgvReviews.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReviews.ReadOnly = true;
            dgvReviews.AllowUserToAddRows = false;
            dgvReviews.RowHeadersVisible = false;

            // Refresh Button
            Button btnRefresh = new Button();
            btnRefresh.Text = "Refresh";
            btnRefresh.Location = new Point(960, 540);
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.BackColor = Color.FromArgb(52, 152, 219);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Click += (s, e) => LoadReviews();

            // Add all to form
            this.Controls.Add(lblTitle);
            this.Controls.Add(grpAddReview);
            this.Controls.Add(lblReviewsList);
            this.Controls.Add(dgvReviews);
            this.Controls.Add(btnRefresh);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading movies: " + ex.Message);
            }
        }

        private void LoadReviews()
        {
            try
            {
                string query = @"
                    SELECT c.FullName AS 'Customer', 
                           m.Title AS 'Movie', 
                           CONCAT(r.Rating, '/5') AS 'Rating',
                           r.Comment AS 'Review'
                    FROM Reviews r
                    JOIN Customers c ON r.CustomerID = c.CustomerID
                    JOIN Movies m ON r.MovieID = m.MovieID
                    ORDER BY r.ReviewID DESC";

                DataTable dt = db.ExecuteQuery(query);
                dgvReviews.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading reviews: " + ex.Message);
            }
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            // Check login
            if (Session.CurrentUser?.CustomerID == null)
            {
                MessageBox.Show("Please login as customer first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check movie selected
            if (cmbMovie.SelectedValue == null)
            {
                MessageBox.Show("Please select a movie!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check comment
            if (string.IsNullOrWhiteSpace(txtComment.Text))
            {
                MessageBox.Show("Please write a comment!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string query = @"
                    INSERT INTO Reviews (CustomerID, MovieID, Rating, Comment) 
                    VALUES (@custID, @movID, @rating, @comment)
                    ON DUPLICATE KEY UPDATE Rating = @rating, Comment = @comment";

                MySqlParameter[] parameters = {
                    new MySqlParameter("@custID", Session.CurrentUser.CustomerID),
                    new MySqlParameter("@movID", Convert.ToInt32(cmbMovie.SelectedValue)),
                    new MySqlParameter("@rating", (int)numRating.Value),
                    new MySqlParameter("@comment", txtComment.Text)
                };

                db.ExecuteNonQuery(query, parameters);

                MessageBox.Show("Review submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear form
                txtComment.Text = "";
                numRating.Value = 5;

                // Refresh reviews
                LoadReviews();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}