public class Showtime
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public int CinemaId { get; set; }
    public int HallId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public decimal BasePrice { get; set; }

    // Navigation Properties
    public Movie Movie { get; set; }
    public Cinema Cinema { get; set; }
    public Hall Hall { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
