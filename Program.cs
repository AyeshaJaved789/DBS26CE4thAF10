using CinemaBookingSystem.Forms;
using System;
using System.Windows.Forms;

namespace CinemaBookingSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}