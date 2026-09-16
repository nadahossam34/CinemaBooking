namespace PresentationLayer.ViewModels
{
    public class ShowtimeViewModel
    {
        public int ShowtimeId { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string MoviePosterUrl { get; set; } = string.Empty;
        public int CinemaId { get; set; }
        public string CinemaName { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TicketPrice { get; set; }
        public int AvailableSeatsCount { get; set; }
    }
}