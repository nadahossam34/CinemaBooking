 public class Booking
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int ShowtimeId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string BookingReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsCheckedIn { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime BookingDate { get; set; }

    // Navigation Properties
    public User? User { get; set; }
    public Showtime Showtime { get; set; }
    public Payment? Payment { get; set; }
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
}