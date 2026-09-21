namespace PresentationLayer.ViewModels
{
    public class SeatTypeViewModel
    {
        public int SeatTypeId { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., Standard, VIP
        public decimal PriceMultiplier { get; set; }
    }
}
