namespace PresentationLayer.ViewModels
{
    public class SeatViewModel
    {
        public int SeatId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string RowLabel { get; set; } = string.Empty;
        public int RowNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string SeatTypeName { get; set; } = string.Empty; // e.g., VIP, Regular
        public decimal Price { get; set; }
        public bool IsBooked { get; set; }
        public bool IsSelected { get; set; }
    }
}