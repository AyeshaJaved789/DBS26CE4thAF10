using System;

namespace CinemaBookingSystem.Models
{
    public class User
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = "";
        public string RoleName { get; set; } = "";
        public int? CustomerID { get; set; }
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Gender { get; set; } = "";
    }

    public class Movie
    {
        public int MovieID { get; set; }
        public int GenreID { get; set; }
        public string GenreName { get; set; } = "";
        public string Title { get; set; } = "";
        public int Duration { get; set; }
        public string Language { get; set; } = "";
        public DateTime ReleaseDate { get; set; }
        public decimal Rating { get; set; }
    }

    public class Show
    {
        public int ShowID { get; set; }
        public int MovieID { get; set; }
        public string MovieTitle { get; set; } = "";
        public int HallID { get; set; }
        public string HallName { get; set; } = "";
        public DateTime ShowDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal TicketPrice { get; set; }
        public int AvailableSeats { get; set; }
        public int TotalSeats { get; set; }
    }

    public class Booking
    {
        public int BookingID { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = "";
        public int ShowID { get; set; }
        public string MovieTitle { get; set; } = "";
        public string HallName { get; set; } = "";
        public DateTime ShowDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Seats { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "";
        public DateTime BookingDate { get; set; }
        public string PaymentMethod { get; set; } = "";
    }

    public class Seat
    {
        public int SeatID { get; set; }
        public int HallID { get; set; }
        public string SeatNumber { get; set; } = "";
        public string SeatType { get; set; } = "";
        public decimal ExtraPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "";
    }

    public class Review
    {
        public int ReviewID { get; set; }
        public int CustomerID { get; set; }
        public int MovieID { get; set; }
        public string MovieTitle { get; set; } = "";
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
    }
}