using System;
using System.Collections.Generic;

namespace BuisnessLogicLayer.ViewModels
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }

        public int UserId { get; set; }

        public int ShowtimeId { get; set; }

        public List<int> SelectedSeatIds { get; set; } = new List<int>();

        public decimal TotalAmount { get; set; }

        public DateTime BookingDate { get; set; }

        public string Status { get; set; } = "Pending";
    }
}