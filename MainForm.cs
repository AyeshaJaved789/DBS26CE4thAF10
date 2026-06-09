using CinemaBookingSystem.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBookingSystem.Forms
{
    public partial class MainForm : Form
    {
        private Panel pnlSidebar;
        private Panel pnlHeader;
        private Panel pnlContent;
        private Label lblWelcome;
        private Label lblRoleBadge;

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        private void InitializeComponent()
        {
            this.pnlSidebar = new Panel();
            this.pnlHeader = new Panel();
            this.pnlContent = new Panel();
            this.SuspendLayout();

            // ── Form ───────────────────────────────────────────
            this.Text = "Cinema Booking System";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 245);
            this.WindowState = FormWindowState.Maximized;

            // ── Sidebar ────────────────────────────────────────
            this.pnlSidebar.Size = new Size(230, 700);
            this.pnlSidebar.BackColor = Color.FromArgb(18, 22, 35);
            this.pnlSidebar.Dock = DockStyle.Left;

            // ── Header ─────────────────────────────────────────
            this.pnlHeader.Size = new Size(800, 60);
            this.pnlHeader.BackColor = Color.FromArgb(26, 31, 46);
            this.pnlHeader.Dock = DockStyle.Top;

            // ── Content ────────────────────────────────────────
            this.pnlContent.BackColor = Color.FromArgb(240, 240, 245);
            this.pnlContent.Dock = DockStyle.Fill;
            this.pnlContent.AutoScroll = true;

            // ── Welcome Label ──────────────────────────────────
            string welcomeName = Session.CurrentUser?.FullName != ""
                                 ? Session.CurrentUser?.FullName
                                 : Session.CurrentUser?.Username ?? "User";

            this.lblWelcome = new Label();
            this.lblWelcome.Text = $"Welcome, {welcomeName}!";
            this.lblWelcome.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            this.lblWelcome.ForeColor = Color.White;
            this.lblWelcome.Size = new Size(500, 35);
            this.lblWelcome.Location = new Point(20, 12);

            // ── Role Badge in Header ───────────────────────────
            string role = Session.CurrentUser?.RoleName ?? "User";
            Color badgeColor = role == "Admin" ? Color.FromArgb(231, 76, 60)
                             : role == "Staff" ? Color.FromArgb(46, 204, 113)
                             : Color.FromArgb(52, 152, 219);

            string roleIcon = role == "Admin" ? "👑"
                            : role == "Staff" ? "🎫" : "👤";

            this.lblRoleBadge = new Label();
            this.lblRoleBadge.Text = $"{roleIcon} {role}";
            this.lblRoleBadge.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.lblRoleBadge.ForeColor = badgeColor;
            this.lblRoleBadge.BackColor = Color.FromArgb(35, 40, 58);
            this.lblRoleBadge.Size = new Size(100, 30);
            this.lblRoleBadge.TextAlign = ContentAlignment.MiddleCenter;
            this.lblRoleBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // Position badge on right side — will be updated on resize
            this.pnlHeader.Resize += (s, e) => {
                this.lblRoleBadge.Location = new Point(this.pnlHeader.Width - 120, 15);
            };
            this.lblRoleBadge.Location = new Point(1080, 15);

            // ── Sidebar Logo ───────────────────────────────────
            Label lblLogo = new Label();
            lblLogo.Text = "🎬 Cinema";
            lblLogo.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            lblLogo.ForeColor = Color.FromArgb(255, 200, 0);
            lblLogo.Size = new Size(230, 60);
            lblLogo.Location = new Point(0, 0);
            lblLogo.TextAlign = ContentAlignment.MiddleCenter;
            lblLogo.BackColor = Color.FromArgb(12, 15, 25);
            this.pnlSidebar.Controls.Add(lblLogo);

            // ── Role Label in Sidebar ──────────────────────────
            Label lblSideRole = new Label();
            lblSideRole.Text = $"{roleIcon} {role} Panel";
            lblSideRole.Font = new Font("Segoe UI", 9);
            lblSideRole.ForeColor = badgeColor;
            lblSideRole.Size = new Size(230, 28);
            lblSideRole.Location = new Point(0, 60);
            lblSideRole.TextAlign = ContentAlignment.MiddleCenter;
            lblSideRole.BackColor = Color.FromArgb(15, 18, 30);
            this.pnlSidebar.Controls.Add(lblSideRole);

            // ── Build Sidebar by Role ──────────────────────────
            int yPos = 98;
            BuildSidebar(role, ref yPos);

            // ── Logout ─────────────────────────────────────────
            Button btnLogout = new Button();
            btnLogout.Text = "🚪  Logout";
            btnLogout.Size = new Size(230, 50);
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.ForeColor = Color.White;
            btnLogout.BackColor = Color.FromArgb(120, 30, 20);
            btnLogout.Font = new Font("Segoe UI", 11);
            btnLogout.TextAlign = ContentAlignment.MiddleCenter;
            btnLogout.Cursor = Cursors.Hand;
            btnLogout.Click += (s, e) => Logout();
            btnLogout.MouseEnter += (s, e) => btnLogout.BackColor = Color.FromArgb(192, 57, 43);
            btnLogout.MouseLeave += (s, e) => btnLogout.BackColor = Color.FromArgb(120, 30, 20);
            this.pnlSidebar.Controls.Add(btnLogout);

            // ── Assemble ───────────────────────────────────────
            this.pnlHeader.Controls.Add(this.lblWelcome);
            this.pnlHeader.Controls.Add(this.lblRoleBadge);

            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlSidebar);
            this.Controls.Add(this.pnlHeader);

            this.ResumeLayout(false);
        }

        // ── Sidebar Builder by Role ────────────────────────────
        private void BuildSidebar(string role, ref int yPos)
        {
            if (role == "Admin")
            {
                // Admin: poora control
                AddSidebarLabel("Main", ref yPos);
                AddSidebarBtn("📊  Dashboard", ref yPos, () => LoadDashboard());
                AddSidebarLabel("Movies ", ref yPos);
                AddSidebarBtn("🎬  Movies", ref yPos, () => LoadMoviesForm());
                AddSidebarLabel("Bookings", ref yPos);
                AddSidebarBtn("📋  All Bookings", ref yPos, () => LoadMyBookingsForm());
                AddSidebarBtn("❌  Cancel Booking", ref yPos, () => LoadCancelBookingForm());
                AddSidebarLabel("Management", ref yPos);
                AddSidebarBtn("📝  Reports", ref yPos, () => LoadReportsForm());
               }
            else if (role == "Staff")
            {
                // Staff: operations
                AddSidebarLabel("Main", ref yPos);
                AddSidebarBtn("📊  Dashboard", ref yPos, () => LoadDashboard());
                AddSidebarLabel("Operations", ref yPos);
                AddSidebarBtn("🎬  Movies", ref yPos, () => LoadMoviesForm());
                AddSidebarBtn("📋  Bookings", ref yPos, () => LoadMyBookingsForm());
                AddSidebarBtn("📝  Reports", ref yPos, () => LoadReportsForm());
            }
            else
            {
           // User: customer options

           //AddSidebarLabel("Main", ref yPos);
           //     AddSidebarBtn("📊  Dashboard", ref yPos, () => LoadDashboard());
                AddSidebarLabel("Movies", ref yPos);
                AddSidebarBtn("🎬  Movies", ref yPos, () => LoadMoviesForm());
                AddSidebarLabel("My Account", ref yPos);
                AddSidebarBtn("🎟  Book Tickets", ref yPos, () => LoadBookingForm());
                AddSidebarBtn("📋  My Bookings", ref yPos, () => LoadMyBookingsForm());
                AddSidebarBtn("❌  Cancel Booking", ref yPos, () => LoadCancelBookingForm());
                AddSidebarBtn("⭐  Reviews", ref yPos, () => LoadReviewsForm());
            }
        }

        // ── Section Label ──────────────────────────────────────
        private void AddSidebarLabel(string text, ref int yPos)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Size = new Size(230, 25);
            lbl.Location = new Point(0, yPos);
            lbl.ForeColor = Color.FromArgb(90, 100, 120);
            lbl.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            lbl.TextAlign = ContentAlignment.MiddleLeft;
            lbl.Padding = new Padding(20, 0, 0, 0);
            lbl.BackColor = Color.Transparent;
            this.pnlSidebar.Controls.Add(lbl);
            yPos += 25;
        }

        // ── Sidebar Button ─────────────────────────────────────
        private void AddSidebarBtn(string text, ref int yPos, Action onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(230, 45);
            btn.Location = new Point(0, yPos);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.FromArgb(200, 205, 215);
            btn.Font = new Font("Segoe UI", 10);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(15, 0, 0, 0);
            btn.BackColor = Color.Transparent;
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => {
                btn.BackColor = Color.FromArgb(35, 42, 58);
                btn.ForeColor = Color.White;
            };
            btn.MouseLeave += (s, e) => {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = Color.FromArgb(200, 205, 215);
            };
            btn.Click += (s, e) => onClick();

            this.pnlSidebar.Controls.Add(btn);
            yPos += 45;
        }

        // ── Load Events ────────────────────────────────────────
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.pnlContent.BringToFront();
            LoadDashboard();
        }

        private void ClearContent()
        {
            this.pnlContent.Controls.Clear();
        }

        private void LoadForm(Form form)
        {
            ClearContent();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            this.pnlContent.Controls.Add(form);
            form.Show();
            this.pnlContent.BringToFront();
        }

        private void LoadDashboard() => LoadForm(new DashboardForm());
        private void LoadMoviesForm() => LoadForm(new MoviesForm());
        private void LoadBookingForm() => LoadForm(new BookingForm());
        private void LoadMyBookingsForm() => LoadForm(new MyBookingsForm());
        private void LoadReviewsForm() => LoadForm(new ReviewsForm());
        private void LoadCancelBookingForm() => LoadForm(new CancelBookingForm());
        private void LoadReportsForm() => LoadForm(new ReportsForm());

        private void Logout()
        {
            Session.Logout();
            this.Close();
            new LoginForm().Show();
        }
    }
}