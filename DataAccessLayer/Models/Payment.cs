public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; }
    public string Status { get; set; }
    public string TransactionReference { get; set; }
    public DateTime PaidAt { get; set; }

    // Navigation Properties
    public Booking Booking { get; set; }
}
