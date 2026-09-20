public class SeatType
{
    public int Id { get; set; }
    public string Name { get; set; }  // e.g., Standard, VIP
    public decimal PriceMultiplier { get; set; }

    // Navigation Properties
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
