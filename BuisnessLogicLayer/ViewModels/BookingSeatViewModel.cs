namespace PresentationLayer.ViewModels
{
    public class BookingSeatViewModel
    {
        public int ShowtimeId { get; set; }
        public List<int> SelectedSeatIds { get; set; } = new List<int>();
        public decimal TotalPrice { get; set; }
    }
}