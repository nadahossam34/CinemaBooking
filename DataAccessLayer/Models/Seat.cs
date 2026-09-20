public class Seat
{
    public int Id { get; set; }
    public int HallId { get; set; }
    public string RowLabel { get; set; }
    public int SeatNumber { get; set; }
    public int SeatTypeId { get; set; }

    // Navigation Properties
    public Hall Hall { get; set; }
    public SeatType SeatType { get; set; }
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
}
