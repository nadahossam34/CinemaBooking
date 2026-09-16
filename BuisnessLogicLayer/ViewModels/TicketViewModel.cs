namespace BuisnessLogicLayer.ViewModels
{
    public class TicketViewModel
    {
        public int BookingId { get; set; }

        public string TicketNumber { get; set; }

        public string MovieTitle { get; set; }

        public string CinemaName { get; set; }

        public string HallName { get; set; }

        public string Showtime { get; set; }

        public string Seats { get; set; }

        public decimal TotalAmount { get; set; }

        public string QRCodeImage { get; set; }
    }
}