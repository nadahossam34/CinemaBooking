using CinemaBooking.Data;

namespace CinemaBooking.ViewModels
{
    public class MoviesIndexViewModel
    {
        public List<Movie> Movies { get; set; } = new();
        public int TotalCatalog { get; set; }
        public int NowShowingCount { get; set; }
        public bool IsTmdbConnected { get; set; }
    }
}
