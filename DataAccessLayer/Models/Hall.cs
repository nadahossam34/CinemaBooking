public class Hall
{
    public int Id { get; set; }
    public int CinemaId { get; set; }
    public string Name { get; set; }
    public int SeatCount { get; set; }

    // Navigation Properties
    public Cinema Cinema { get; set; }
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}