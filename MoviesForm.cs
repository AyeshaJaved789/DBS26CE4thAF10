using CinemaBookingSystem.Database;
using CinemaBookingSystem.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBookingSystem.Forms
{
    public partial class MoviesForm : Form
    {
        private DatabaseHelper db;
        private DataGridView dgvMovies;
        private ComboBox cmbGenre;
        private TextBox txtSearch;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        public MoviesForm()
        {
            InitializeComponent();
            db = new DatabaseHelper();
            LoadGenres();
            LoadMovies();
            ApplyRolePermissions();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 600);
            this.BackColor = Color.FromArgb(240, 240, 245);

            // ── Title ──────────────────────────────────────────
            Label lblTitle = new Label();
            lblTitle.Text = "🎬 Movies";
            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Size = new Size(300, 50);
            lblTitle.Location = new Point(20, 20);

            // ── Search ─────────────────────────────────────────
            Label lblSearch = new Label();
            lblSearch.Text = "Search:";
            lblSearch.Location = new Point(20, 90);
            lblSearch.Size = new Size(60, 25);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(80, 88);
            txtSearch.Size = new Size(200, 25);
            txtSearch.TextChanged += (s, e) => LoadMovies();

            // ── Genre Filter ───────────────────────────────────
            Label lblGenre = new Label();
            lblGenre.Text = "Genre:";
            lblGenre.Location = new Point(300, 90);
            lblGenre.Size = new Size(50, 25);

            cmbGenre = new ComboBox();
            cmbGenre.Location = new Point(350, 88);
            cmbGenre.Size = new Size(150, 30);
            cmbGenre.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGenre.SelectedIndexChanged += (s, e) => LoadMovies();

            // ── Admin Buttons ──────────────────────────────────
            btnAdd = new Button();
            btnAdd.Text = "➕ Add Movie";
            btnAdd.Size = new Size(120, 32);
            btnAdd.Location = new Point(520, 84);
            btnAdd.BackColor = Color.FromArgb(46, 204, 113);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Visible = false;
            btnAdd.Click += btnAdd_Click;

            btnEdit = new Button();
            btnEdit.Text = "✏ Edit";
            btnEdit.Size = new Size(90, 32);
            btnEdit.Location = new Point(650, 84);
            btnEdit.BackColor = Color.FromArgb(52, 152, 219);
            btnEdit.ForeColor = Color.White;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnEdit.Cursor = Cursors.Hand;
            btnEdit.Visible = false;
            btnEdit.Click += btnEdit_Click;

            btnDelete = new Button();
            btnDelete.Text = "🗑 Delete";
            btnDelete.Size = new Size(100, 32);
            btnDelete.Location = new Point(750, 84);
            btnDelete.BackColor = Color.FromArgb(231, 76, 60);
            btnDelete.ForeColor = Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Visible = false;
            btnDelete.Click += btnDelete_Click;

            // ── DataGridView ───────────────────────────────────
            dgvMovies = new DataGridView();
            dgvMovies.Location = new Point(20, 130);
            dgvMovies.Size = new Size(950, 430);
            dgvMovies.BackgroundColor = Color.White;
            dgvMovies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMovies.ReadOnly = true;
            dgvMovies.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMovies.RowHeadersVisible = false;
            dgvMovies.BorderStyle = BorderStyle.None;
            dgvMovies.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvMovies.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvMovies.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 62, 80);
            dgvMovies.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvMovies.EnableHeadersVisualStyles = false;
            dgvMovies.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 250);

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblSearch);
            this.Controls.Add(txtSearch);
            this.Controls.Add(lblGenre);
            this.Controls.Add(cmbGenre);
            this.Controls.Add(btnAdd);
            this.Controls.Add(btnEdit);
            this.Controls.Add(btnDelete);
            this.Controls.Add(dgvMovies);
        }

        // ── Role Check ─────────────────────────────────────────
        private void ApplyRolePermissions()
        {
            string role = Session.CurrentUser?.RoleName ?? "User";
            bool isAdmin = role == "Admin";
            btnAdd.Visible = isAdmin;
            btnEdit.Visible = isAdmin;
            btnDelete.Visible = isAdmin;
        }

        // ── Load Data ──────────────────────────────────────────
        private void LoadGenres()
        {
            string query = "SELECT GenreID, GenreName FROM Genres";
            DataTable dt = db.ExecuteQuery(query);
            DataRow allRow = dt.NewRow();
            allRow["GenreID"] = 0;
            allRow["GenreName"] = "-- All Genres --";
            dt.Rows.InsertAt(allRow, 0);
            cmbGenre.DisplayMember = "GenreName";
            cmbGenre.ValueMember = "GenreID";
            cmbGenre.DataSource = dt;
        }

        private void LoadMovies()
        {
            string query = @"
                SELECT m.MovieID, m.Title, g.GenreName AS Genre,
                       m.Duration, m.Language, m.ReleaseDate, m.Rating
                FROM Movies m
                JOIN Genres g ON m.GenreID = g.GenreID
                WHERE (@search = '' OR m.Title LIKE @search)
                  AND (@genreID = 0 OR m.GenreID = @genreID)
                ORDER BY m.Title";

            int genreID = cmbGenre.SelectedValue != null
                          ? Convert.ToInt32(cmbGenre.SelectedValue) : 0;

            MySqlParameter[] parameters = {
                new MySqlParameter("@search",  $"%{txtSearch.Text}%"),
                new MySqlParameter("@genreID", genreID)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            dgvMovies.DataSource = dt;

            if (dgvMovies.Columns["MovieID"] != null)
                dgvMovies.Columns["MovieID"].Visible = false;
        }

        // ── Admin Actions ──────────────────────────────────────
        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddEditMovieForm addForm = new AddEditMovieForm();
            if (addForm.ShowDialog() == DialogResult.OK)
                LoadMovies();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvMovies.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pehle koi movie select karein!", "Edit",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int movieID = Convert.ToInt32(dgvMovies.SelectedRows[0].Cells["MovieID"].Value);
            AddEditMovieForm editForm = new AddEditMovieForm(movieID);
            if (editForm.ShowDialog() == DialogResult.OK)
                LoadMovies();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvMovies.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pehle koi movie select karein!", "Delete",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string movieTitle = dgvMovies.SelectedRows[0].Cells["Title"].Value.ToString();
            int movieID = Convert.ToInt32(dgvMovies.SelectedRows[0].Cells["MovieID"].Value);

            DialogResult confirm = MessageBox.Show(
                $"'{movieTitle}' ko delete karna chahte hain?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                string deleteQuery = "DELETE FROM Movies WHERE MovieID = @movieID";
                MySqlParameter[] p = { new MySqlParameter("@movieID", movieID) };
                db.ExecuteNonQuery(deleteQuery, p);
                MessageBox.Show("✅ Movie delete ho gayi!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadMovies();
            }
        }
    } // MoviesForm class end
} // namespace end