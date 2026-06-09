using CinemaBookingSystem.Models;

namespace CinemaBookingSystem.Utils
{
    public static class Session
    {
        public static User CurrentUser { get; set; }

        public static bool IsLoggedIn => CurrentUser != null;

        public static bool IsAdmin => CurrentUser != null && CurrentUser.RoleName == "Admin";

        public static bool IsStaff => CurrentUser != null && CurrentUser.RoleName == "Staff";

        public static bool IsCustomer => CurrentUser != null && CurrentUser.RoleName == "Customer";

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}