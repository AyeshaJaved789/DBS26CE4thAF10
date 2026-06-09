using CinemaBookingSystem.Database;
using CinemaBookingSystem.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBookingSystem.Forms
{
    public partial class CancelBookingForm : Form
    {
        private DatabaseHelper db;
        private TextBox txtBookingID;
        private Button btnCancel;
        private Label lblRefund;
        private Label lblResult;

        public CancelBookingForm()
        {
            InitializeComponent();
            db = new DatabaseHelper();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(550, 380);
            this.BackColor = Color.FromArgb(240, 240, 245);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitle = new Label();
            lblTitle.Text = "Cancel Booking";
            lblTitle.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Size = new Size(300, 45);
            lblTitle.Location = new Point(120, 20);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            Label lblBooking = new Label();
            lblBooking.Text = "Enter Booking ID:";
            lblBooking.Font = new Font("Segoe UI", 12);
            lblBooking.Location = new Point(60, 100);
            lblBooking.Size = new Size(140, 30);

            txtBookingID = new TextBox();
            txtBookingID.Size = new Size(200, 30);
            txtBookingID.Location = new Point(210, 100);
            txtBookingID.Font = new Font("Segoe UI", 12);

            btnCancel = new Button();
            btnCancel.Text = "Cancel Booking";
            btnCancel.Size = new Size(150, 40);
            btnCancel.Location = new Point(190, 160);
            btnCancel.BackColor = Color.FromArgb(231, 76, 60);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnCancel.Click += BtnCancel_Click;

            lblRefund = new Label();
            lblRefund.Text = "";
            lblRefund.Font = new Font("Segoe UI", 11);
            lblRefund.Location = new Point(50, 220);
            lblRefund.Size = new Size(450, 60);
            lblRefund.TextAlign = ContentAlignment.MiddleCenter;

            lblResult = new Label();
            lblResult.Text = "";
            lblResult.Font = new Font("Segoe UI", 10);
            lblResult.ForeColor = Color.Gray;
            lblResult.Location = new Point(50, 290);
            lblResult.Size = new Size(450, 30);
            lblResult.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblBooking);
            this.Controls.Add(txtBookingID);
            this.Controls.Add(btnCancel);
            this.Controls.Add(lblRefund);
            this.Controls.Add(lblResult);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBookingID.Text))
            {
                MessageBox.Show("Please enter Booking ID!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtBookingID.Text, out int bookingID))
            {
                MessageBox.Show("Please enter a valid numeric Booking ID!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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

                        lblRefund.Text = message;
                        lblResult.Text = refund > 0 ? $"Refund Amount: Rs. {refund:N2}" : "No refund applicable";

                        MessageBox.Show(message, "Cancellation", MessageBoxButtons.OK,
                            message.Contains("successfully") ? MessageBoxIcon.Information : MessageBoxIcon.Information);

                        if (message.Contains("successfully"))
                        {
                            txtBookingID.Text = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}