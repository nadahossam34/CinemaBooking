namespace PresentationLayer.ViewModels
{
    public class SeatMapViewModel
    {
        public int ShowtimeId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string CinemaName { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public decimal BasePrice { get; set; }

        public List<SeatViewModel> Seats { get; set; } = new List<SeatViewModel>();
    }
}