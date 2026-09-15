using System.Collections.Generic;

namespace BuisnessLogicLayer.ViewModels
{
    public class CheckoutViewModel
    {
        public int ShowtimeId { get; set; }

        public string MovieTitle { get; set; }

        public string CinemaName { get; set; }

        public string HallName { get; set; }

        public string Showtime { get; set; }

        public List<int> SelectedSeatIds { get; set; } = new List<int>();

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }

        public decimal TotalAmount { get; set; }
    }
}