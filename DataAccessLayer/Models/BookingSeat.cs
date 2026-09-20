public class BookingSeat
{
    public int BookingId { get; set; }
    public int SeatId { get; set; }
    public decimal PricePaid { get; set; }

    // Navigation Properties
    public Booking Booking { get; set; }
    public Seat Seat { get; set; }
}
