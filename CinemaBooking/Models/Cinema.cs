public class Cinema
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string City { get; set; }
    public string Address { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    // Navigation Properties
    public ICollection<Hall> Halls { get; set; } = new List<Hall>();
    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
