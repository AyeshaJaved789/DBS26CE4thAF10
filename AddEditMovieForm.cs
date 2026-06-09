using CinemaBookingSystem.Database;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBookingSystem.Forms
{
    public class AddEditMovieForm : Form
    {
        private DatabaseHelper db;
        private int movieID = 0; // 0 = Add, >0 = Edit

        private TextBox txtTitle;
        private ComboBox cmbGenre;
        private TextBox txtDuration;
        private TextBox txtLanguage;
        private DateTimePicker dtpReleaseDate;
        private TextBox txtRating;
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnCancel;

        // Add mode
        public AddEditMovieForm()
        {
            movieID = 0;
            InitializeComponent();
            db = new DatabaseHelper();
            LoadGenres();
            this.Text = "Add New Movie";
        }

        // Edit mode
        public AddEditMovieForm(int editMovieID)
        {
            movieID = editMovieID;
            InitializeComponent();
            db = new DatabaseHelper();
            LoadGenres();
            LoadMovieData();
            this.Text = "Edit Movie";
        }

        private void InitializeComponent()
        {
            this.Size = new Size(480, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 240, 245);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // ── Title ──────────────────────────────────────────
            Label lblTitle = new Label();
            lblTitle.Text = movieID == 0 ? "➕ Add New Movie" : "✏ Edit Movie";
            lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Size = new Size(440, 45);
            lblTitle.Location = new Point(20, 15);

            // ── Movie Title ────────────────────────────────────
            Label lblMovieTitle = new Label();
            lblMovieTitle.Text = "Movie Title *";
            lblMovieTitle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblMovieTitle.Location = new Point(20, 70);
            lblMovieTitle.Size = new Size(200, 25);

            txtTitle = new TextBox();
            txtTitle.Size = new Size(420, 30);
            txtTitle.Location = new Point(20, 93);
            txtTitle.Font = new Font("Segoe UI", 11);

            // ── Genre ──────────────────────────────────────────
            Label lblGenre = new Label();
            lblGenre.Text = "Genre *";
            lblGenre.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblGenre.Location = new Point(20, 133);
            lblGenre.Size = new Size(200, 25);

            cmbGenre = new ComboBox();
            cmbGenre.Size = new Size(200, 30);
            cmbGenre.Location = new Point(20, 156);
            cmbGenre.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGenre.Font = new Font("Segoe UI", 11);

            // ── Duration ───────────────────────────────────────
            Label lblDuration = new Label();
            lblDuration.Text = "Duration (minutes) *";
            lblDuration.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblDuration.Location = new Point(240, 133);
            lblDuration.Size = new Size(200, 25);

            txtDuration = new TextBox();
            txtDuration.Size = new Size(200, 30);
            txtDuration.Location = new Point(240, 156);
            txtDuration.Font = new Font("Segoe UI", 11);
            txtDuration.Text = ""; // e.g. 120

            // ── Language ───────────────────────────────────────
            Label lblLanguage = new Label();
            lblLanguage.Text = "Language *";
            lblLanguage.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblLanguage.Location = new Point(20, 196);
            lblLanguage.Size = new Size(200, 25);

            txtLanguage = new TextBox();
            txtLanguage.Size = new Size(200, 30);
            txtLanguage.Location = new Point(20, 219);
            txtLanguage.Font = new Font("Segoe UI", 11);
            txtLanguage.Text = ""; // e.g. Urdu, English

            // ── Rating ─────────────────────────────────────────
            Label lblRating = new Label();
            lblRating.Text = "Rating (0-10)";
            lblRating.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblRating.Location = new Point(240, 196);
            lblRating.Size = new Size(200, 25);

            txtRating = new TextBox();
            txtRating.Size = new Size(200, 30);
            txtRating.Location = new Point(240, 219);
            txtRating.Font = new Font("Segoe UI", 11);
            txtRating.Text = ""; // e.g. 8.5

            // ── Release Date ───────────────────────────────────
            Label lblRelease = new Label();
            lblRelease.Text = "Release Date *";
            lblRelease.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblRelease.Location = new Point(20, 259);
            lblRelease.Size = new Size(200, 25);

            dtpReleaseDate = new DateTimePicker();
            dtpReleaseDate.Size = new Size(200, 30);
            dtpReleaseDate.Location = new Point(20, 282);
            dtpReleaseDate.Font = new Font("Segoe UI", 11);
            dtpReleaseDate.Format = DateTimePickerFormat.Short;

            // ── Description ────────────────────────────────────
            Label lblDesc = new Label();
            lblDesc.Text = "Description";
            lblDesc.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblDesc.Location = new Point(20, 322);
            lblDesc.Size = new Size(200, 25);

            txtDescription = new TextBox();
            txtDescription.Size = new Size(420, 70);
            txtDescription.Location = new Point(20, 345);
            txtDescription.Font = new Font("Segoe UI", 10);
            txtDescription.Multiline = true;
            txtDescription.ScrollBars = ScrollBars.Vertical;

            // ── Buttons ────────────────────────────────────────
            btnSave = new Button();
            btnSave.Text = movieID == 0 ? "✅ Save Movie" : "✅ Update Movie";
            btnSave.Size = new Size(180, 42);
            btnSave.Location = new Point(20, 435);
            btnSave.BackColor = Color.FromArgb(46, 204, 113);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnSave.Cursor = Cursors.Hand;
            btnSave.Click += btnSave_Click;

            btnCancel = new Button();
            btnCancel.Text = "❌ Cancel";
            btnCancel.Size = new Size(150, 42);
            btnCancel.Location = new Point(215, 435);
            btnCancel.BackColor = Color.FromArgb(231, 76, 60);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Font = new Font("Segoe UI", 11);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += (s, e) => this.Close();

            // ── Add Controls ───────────────────────────────────
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblMovieTitle);
            this.Controls.Add(txtTitle);
            this.Controls.Add(lblGenre);
            this.Controls.Add(cmbGenre);
            this.Controls.Add(lblDuration);
            this.Controls.Add(txtDuration);
            this.Controls.Add(lblLanguage);
            this.Controls.Add(txtLanguage);
            this.Controls.Add(lblRating);
            this.Controls.Add(txtRating);
            this.Controls.Add(lblRelease);
            this.Controls.Add(dtpReleaseDate);
            this.Controls.Add(lblDesc);
            this.Controls.Add(txtDescription);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        // ── Load Genres ────────────────────────────────────────
        private void LoadGenres()
        {
            string query = "SELECT GenreID, GenreName FROM Genres ORDER BY GenreName";
            DataTable dt = db.ExecuteQuery(query);
            cmbGenre.DisplayMember = "GenreName";
            cmbGenre.ValueMember = "GenreID";
            cmbGenre.DataSource = dt;
        }

        // ── Load Movie Data for Edit ───────────────────────────
        private void LoadMovieData()
        {
            string query = "SELECT * FROM Movies WHERE MovieID = @movieID";
            MySqlParameter[] p = { new MySqlParameter("@movieID", movieID) };
            DataTable dt = db.ExecuteQuery(query, p);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                txtTitle.Text = row["Title"].ToString();
                txtDuration.Text = row["Duration"].ToString();
                txtLanguage.Text = row["Language"].ToString();
                txtRating.Text = row["Rating"].ToString();
                txtDescription.Text = row["Description"] != DBNull.Value
                                      ? row["Description"].ToString() : "";

                if (row["ReleaseDate"] != DBNull.Value)
                    dtpReleaseDate.Value = Convert.ToDateTime(row["ReleaseDate"]);

                // Genre select karo
                if (row["GenreID"] != DBNull.Value)
                {
                    int genreID = Convert.ToInt32(row["GenreID"]);
                    foreach (DataRowView item in cmbGenre.Items)
                    {
                        if (Convert.ToInt32(item["GenreID"]) == genreID)
                        {
                            cmbGenre.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        // ── Save / Update ──────────────────────────────────────
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("⚠ Movie title zaroor daalen!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitle.Focus();
                return;
            }

            if (cmbGenre.SelectedValue == null)
            {
                MessageBox.Show("⚠ Genre select karein!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtDuration.Text, out int duration) || duration <= 0)
            {
                MessageBox.Show("⚠ Duration sahi number daalen! (e.g. 120)", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDuration.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLanguage.Text))
            {
                MessageBox.Show("⚠ Language zaroor daalen!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLanguage.Focus();
                return;
            }

            decimal rating = 0;
            if (!string.IsNullOrWhiteSpace(txtRating.Text))
            {
                if (!decimal.TryParse(txtRating.Text, out rating) || rating < 0 || rating > 10)
                {
                    MessageBox.Show("⚠ Rating 0 se 10 ke darmiyan honi chahiye!", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtRating.Focus();
                    return;
                }
            }

            try
            {
                if (movieID == 0)
                {
                    //// ── ADD ────────────────────────────────────
               
                    string query = @"
    INSERT INTO Movies (Title, GenreID, Duration, Language, ReleaseDate, Rating)
    VALUES (@title, @genreID, @duration, @language, @releaseDate, @rating)";

                    MySqlParameter[] p = {
    new MySqlParameter("@title",       txtTitle.Text.Trim()),
    new MySqlParameter("@genreID",     Convert.ToInt32(cmbGenre.SelectedValue)),
    new MySqlParameter("@duration",    duration),
    new MySqlParameter("@language",    txtLanguage.Text.Trim()),
    new MySqlParameter("@releaseDate", dtpReleaseDate.Value.Date),
    new MySqlParameter("@rating",      rating)
};

                    db.ExecuteNonQuery(query, p);
                    MessageBox.Show("✅ Movie successfully add ho gayi!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // ── EDIT ───────────────────────────────────
                    string query = @"
                        UPDATE Movies 
                        SET Title = @title, GenreID = @genreID, Duration = @duration,
                            Language = @language, ReleaseDate = @releaseDate,
                            Rating = @rating, Description = @description
                        WHERE MovieID = @movieID";

                    MySqlParameter[] p = {
                        new MySqlParameter("@title",       txtTitle.Text.Trim()),
                        new MySqlParameter("@genreID",     Convert.ToInt32(cmbGenre.SelectedValue)),
                        new MySqlParameter("@duration",    duration),
                        new MySqlParameter("@language",    txtLanguage.Text.Trim()),
                        new MySqlParameter("@releaseDate", dtpReleaseDate.Value.Date),
                        new MySqlParameter("@rating",      rating),
                        new MySqlParameter("@description", txtDescription.Text.Trim()),
                        new MySqlParameter("@movieID",     movieID)
                    };

                    db.ExecuteNonQuery(query, p);
                    MessageBox.Show("✅ Movie successfully update ho gayi!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    } // AddEditMovieForm class end
} // namespace end