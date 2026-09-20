namespace CinemaBooking.ViewModels
{
    /// <summary>
    /// Not explicitly requested in the task list, but added so the Profile view
    /// never has direct access to the User entity (and therefore can never
    /// accidentally render PasswordHash). Only the fields safe to display.
    /// </summary>
    public class ProfileViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsAdmin { get; set; }
        public List<Booking> Bookings { get; set; } = new();
    }
}
