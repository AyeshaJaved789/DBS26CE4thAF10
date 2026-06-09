using CinemaBookingSystem.Database;
using CinemaBookingSystem.Models;
using CinemaBookingSystem.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
 
namespace CinemaBookingSystem.Forms
{
    public partial class LoginForm : Form
    {
        private DatabaseHelper db;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnBack;
        private Label lblError;
        private Label lblSelectedRole;
        private Panel pnlRoleSelection;
        private Panel pnlLoginForm;
        private TableLayoutPanel mainLayout;
        private string selectedRole = "";

        public LoginForm()
        {
            InitializeComponent();
            db = new DatabaseHelper();
        }

        private void InitializeComponent()
        {
            this.Text = "Cinema Booking System - Login";
            this.Size = new Size(480, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(18, 18, 30);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.WindowState = FormWindowState.Maximized;
            this.Font = new Font("Segoe UI", 9);

            // ── Main TableLayoutPanel (fills whole form) ───────
            mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 4;
            mainLayout.ColumnCount = 3;

            // Columns: left spacer | center content | right spacer
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            // Rows: top space | title | roles | login form | bottom space
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));  // top space
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 18F));  // title
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 32F));  // role cards
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));  // login form
            // ── Title Panel ────────────────────────────────────
            Panel pnlTitle = new Panel();
            pnlTitle.Dock = DockStyle.Fill;
            pnlTitle.BackColor = Color.Transparent;

            Label lblTitle = new Label();
            lblTitle.Text = "🎬 Cinema Booking System";
            lblTitle.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(255, 200, 0);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 55;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Apna role select karein login karne ke liye";
            lblSubtitle.Font = new Font("Segoe UI", 11);
            lblSubtitle.ForeColor = Color.FromArgb(160, 160, 180);
            lblSubtitle.Dock = DockStyle.Top;
            lblSubtitle.Height = 35;
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;

            pnlTitle.Controls.Add(lblSubtitle);
            pnlTitle.Controls.Add(lblTitle);

            // ── Role Selection Panel ───────────────────────────
            pnlRoleSelection = new Panel();
            pnlRoleSelection.Dock = DockStyle.Fill;
            pnlRoleSelection.BackColor = Color.Transparent;

            TableLayoutPanel roleLayout = new TableLayoutPanel();
            roleLayout.Dock = DockStyle.Fill;
            roleLayout.ColumnCount = 3;
            roleLayout.RowCount = 1;
            roleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            roleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            roleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.4F));
            roleLayout.Padding = new Padding(10);
            roleLayout.BackColor = Color.Transparent;

            Button btnAdmin = CreateRoleButton("👑", "Admin", "Full system control", Color.FromArgb(231, 76, 60));
            btnAdmin.Click += (s, e) => SelectRole("Admin");

            Button btnStaff = CreateRoleButton("🎫", "Staff", "Ticket & shows manage", Color.FromArgb(46, 204, 113));
            btnStaff.Click += (s, e) => SelectRole("Staff");

            Button btnUser = CreateRoleButton("👤", "Customer", "Book your tickets", Color.FromArgb(52, 152, 219));
            btnUser.Click += (s, e) => SelectRole("Customer");

            roleLayout.Controls.Add(btnAdmin, 0, 0);
            roleLayout.Controls.Add(btnStaff, 1, 0);
            roleLayout.Controls.Add(btnUser, 2, 0);

            pnlRoleSelection.Controls.Add(roleLayout);

            // ── Login Form Panel ───────────────────────────────
            pnlLoginForm = new Panel();
            pnlLoginForm.Dock = DockStyle.Fill;
            pnlLoginForm.BackColor = Color.Transparent;
            pnlLoginForm.Visible = false;

            // Inner centered panel
            Panel pnlInner = new Panel();
            pnlInner.BackColor = Color.FromArgb(28, 28, 45);
            pnlInner.Padding = new Padding(30);
            pnlInner.Anchor = AnchorStyles.None;
            pnlInner.Size = new Size(420, 260);


            lblSelectedRole = new Label();
            lblSelectedRole.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lblSelectedRole.ForeColor = Color.White;
            lblSelectedRole.Size = new Size(360, 35);
            lblSelectedRole.Location = new Point(30, 15);
            lblSelectedRole.TextAlign = ContentAlignment.MiddleLeft;

            Label lblUser = new Label();
            lblUser.Text = "Username";
            lblUser.ForeColor = Color.FromArgb(160, 160, 180);
            lblUser.Font = new Font("Segoe UI", 9);
            lblUser.Size = new Size(360, 20);
            lblUser.Location = new Point(30, 58);

            txtUsername = new TextBox();
            txtUsername.Size = new Size(360, 30);
            txtUsername.Location = new Point(30, 78);
            txtUsername.Font = new Font("Segoe UI", 11);
            txtUsername.BackColor = Color.FromArgb(40, 40, 60);
            txtUsername.ForeColor = Color.White;
            txtUsername.BorderStyle = BorderStyle.FixedSingle;

            Label lblPass = new Label();
            lblPass.Text = "Password";
            lblPass.ForeColor = Color.FromArgb(160, 160, 180);
            lblPass.Font = new Font("Segoe UI", 9);
            lblPass.Size = new Size(360, 20);
            lblPass.Location = new Point(30, 115);

            txtPassword = new TextBox();
            txtPassword.Size = new Size(360, 30);
            txtPassword.Location = new Point(30, 135);
            txtPassword.Font = new Font("Segoe UI", 11);
            txtPassword.BackColor = Color.FromArgb(40, 40, 60);
            txtPassword.ForeColor = Color.White;
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.PasswordChar = '●';

            lblError = new Label();
            lblError.ForeColor = Color.FromArgb(231, 76, 60);
            lblError.Size = new Size(360, 20);
            lblError.Location = new Point(30, 172);
            lblError.Text = "";
            lblError.Font = new Font("Segoe UI", 9);

            btnLogin = new Button();
            btnLogin.Text = "Login →";
            btnLogin.Size = new Size(165, 40);
            btnLogin.Location = new Point(30, 198);
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnLogin.Click += btnLogin_Click;
            btnLogin.Cursor = Cursors.Hand;

            btnBack = new Button();
            btnBack.Text = "Logout";
            btnBack.Size = new Size(165, 40);
            btnBack.Location = new Point(205, 198);
            btnBack.BackColor = Color.FromArgb(50, 50, 70);
            btnBack.ForeColor = Color.FromArgb(160, 160, 180);
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Font = new Font("Segoe UI", 10);
            btnBack.Click += btnBack_Click;
            btnBack.Cursor = Cursors.Hand;

            pnlInner.Controls.Add(lblSelectedRole);
            pnlInner.Controls.Add(lblUser);
            pnlInner.Controls.Add(txtUsername);
            pnlInner.Controls.Add(lblPass);
            pnlInner.Controls.Add(txtPassword);
            pnlInner.Controls.Add(lblError);
            pnlInner.Controls.Add(btnLogin);
            pnlInner.Controls.Add(btnBack);

            pnlLoginForm.Controls.Add(pnlInner);

            // Center pnlInner when form resizes
            pnlLoginForm.Resize += (s, e) =>
            {
                pnlInner.Location = new Point(
                    (pnlLoginForm.Width - pnlInner.Width) / 2,
                    (pnlLoginForm.Height - pnlInner.Height) / 2
                );
            };

            // ── Exit Button ────────────────────────────────────
            Panel pnlBottom = new Panel();
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Height = 50;
            pnlBottom.BackColor = Color.Transparent;

            Button btnExitApp = new Button();
            btnExitApp.Text = "✕ Application Band Karein";
            btnExitApp.Size = new Size(200, 32);
            btnExitApp.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExitApp.Location = new Point(pnlBottom.Width - 210, 9);
            btnExitApp.BackColor = Color.FromArgb(60, 30, 30);
            btnExitApp.ForeColor = Color.FromArgb(231, 76, 60);
            btnExitApp.FlatStyle = FlatStyle.Flat;
            btnExitApp.FlatAppearance.BorderSize = 0;
            btnExitApp.Font = new Font("Segoe UI", 9);
            btnExitApp.Click += (s, e) => Application.Exit();
            btnExitApp.Cursor = Cursors.Hand;
            pnlBottom.Controls.Add(btnExitApp);

            pnlBottom.Resize += (s, e) =>
            {
                btnExitApp.Location = new Point(pnlBottom.Width - 210, 9);
            };

            // ── Assemble Layout ────────────────────────────────
            mainLayout.Controls.Add(pnlTitle, 1, 1);
            mainLayout.Controls.Add(pnlRoleSelection, 1, 2);
            mainLayout.Controls.Add(pnlLoginForm, 1, 3);

            this.Controls.Add(mainLayout);
            this.Controls.Add(pnlBottom);
        }

        // ── Role Button Factory ────────────────────────────────
        private Button CreateRoleButton(string icon, string role, string desc, Color accent)
        {
            Button btn = new Button();
            btn.Dock = DockStyle.Fill;
            btn.Margin = new Padding(10);
            btn.BackColor = Color.FromArgb(28, 28, 45);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = accent;
            btn.FlatAppearance.BorderSize = 2;
            btn.Cursor = Cursors.Hand;
            btn.Text = $"{icon}\r\n{role}\r\n{desc}";
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.TextAlign = ContentAlignment.MiddleCenter;

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(
                    Math.Min(accent.R + 30, 255),
                    Math.Min(accent.G + 15, 255),
                    Math.Min(accent.B + 15, 255));
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = Color.FromArgb(28, 28, 45);
            };

            return btn;
        }

        // ── Role Selected ──────────────────────────────────────
        private void SelectRole(string role)
        {
            selectedRole = role;
            lblError.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";

            Color accent;
            string icon;
            switch (role)
            {
                case "Admin": accent = Color.FromArgb(231, 76, 60); icon = "👑"; break;
                case "Staff": accent = Color.FromArgb(46, 204, 113); icon = "🎫"; break;
                default: accent = Color.FromArgb(52, 152, 219); icon = "👤"; break;
            }

            lblSelectedRole.Text = $"{icon} {role} Login";
            lblSelectedRole.ForeColor = accent;
            btnLogin.BackColor = accent;

            pnlLoginForm.Visible = true;
            txtUsername.Focus();
        }

        // ── Back Button ────────────────────────────────────────
        private void btnBack_Click(object sender, EventArgs e)
        {
            pnlLoginForm.Visible = false;
            selectedRole = "";
        }

        // ── Login Logic ────────────────────────────────────────
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "⚠ Username aur password daalein!";
                return;
            }

            string query = @"
                SELECT u.UserID, u.RoleID, u.Username, u.Email, r.RoleName,
                       c.CustomerID, c.FullName, c.Phone, c.Gender
                FROM Users u
                JOIN Roles r ON u.RoleID = r.RoleID
                LEFT JOIN Customers c ON u.UserID = c.UserID
                WHERE u.Username = @username 
                  AND u.Password = @password
                  AND r.RoleName = @roleName";

            MySqlParameter[] parameters = {
                new MySqlParameter("@username", username),
                new MySqlParameter("@password", password),
                new MySqlParameter("@roleName", selectedRole)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                Session.CurrentUser = new User
                {
                    UserID = Convert.ToInt32(row["UserID"]),
                    RoleID = Convert.ToInt32(row["RoleID"]),
                    Username = row["Username"].ToString(),
                    Email = row["Email"].ToString(),
                    RoleName = row["RoleName"].ToString(),
                    CustomerID = row["CustomerID"] != DBNull.Value
                                   ? Convert.ToInt32(row["CustomerID"]) : (int?)null,
                    FullName = row["FullName"] != DBNull.Value ? row["FullName"].ToString() : "",
                    Phone = row["Phone"] != DBNull.Value ? row["Phone"].ToString() : "",
                    Gender = row["Gender"] != DBNull.Value ? row["Gender"].ToString() : ""
                };

                this.Hide();
                MainForm mainForm = new MainForm();
                mainForm.ShowDialog();
                this.Close();
            }
            else
            {
                lblError.Text = $"⚠ {selectedRole} credentials galat hain!";
            }
        }
    }
}