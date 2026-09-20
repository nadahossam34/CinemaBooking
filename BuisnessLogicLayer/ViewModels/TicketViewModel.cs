namespace BuisnessLogicLayer.ViewModels
{
    public class TicketViewModel
    {
        public int BookingId { get; set; }

        public string TicketNumber { get; set; } = string.Empty;

        public string MovieTitle { get; set; } = string.Empty;

        public string CinemaName { get; set; } = string.Empty;

        public string HallName { get; set; } = string.Empty;

        public string Showtime { get; set; } = string.Empty;

        public string Seats { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string QRCodeImage { get; set; } = string.Empty;
    }
}