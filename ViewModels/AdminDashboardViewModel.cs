namespace CinemaBooking.ViewModels
{
    /// <summary>
    /// Real, database-derived admin dashboard stats. Every field is populated
    /// by AdminController.Index() from actual EF Core queries - none of these
    /// numbers are hardcoded or mocked.
    /// </summary>
    public class AdminDashboardViewModel
    {
        public int TotalMovies { get; set; }
        public int TotalCinemas { get; set; }
        public int TotalUsers { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int UpcomingShowtimesCount { get; set; }
        public int TicketsSold { get; set; }

        public List<global::Booking> RecentBookings { get; set; } = new List<global::Booking>();
        public List<global::Movie> RecentMovies { get; set; } = new List<global::Movie>();
    }
}
