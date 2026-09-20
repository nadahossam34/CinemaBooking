public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
