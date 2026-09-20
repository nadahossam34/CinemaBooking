using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BuisnessLogicLayer.ViewModels
{
    public class CheckoutViewModel
    {
        public int ShowtimeId { get; set; }

        public string? MovieTitle { get; set; }

        public string? CinemaName { get; set; }

        public string? HallName { get; set; }

        public string? Showtime { get; set; }

        public List<int> SelectedSeatIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "Full Name is required.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string CustomerEmail { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
    }
}