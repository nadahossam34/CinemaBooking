using BuisnessLogicLayer.Service;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.ViewModels;

namespace PresentationLayer.Controllers
{
    public class ShowtimesController : Controller
    {
        private readonly IShowtimeService _showtimeService;

        public ShowtimesController(IShowtimeService showtimeService)
        {
            _showtimeService = showtimeService;
        }

        public async Task<IActionResult> Index(int movieId)
        {
            var showtimes = await _showtimeService.GetShowtimesByMovieIdAsync(movieId);

            var viewModelList = showtimes.Select(s => new ShowtimeViewModel
            {
                ShowtimeId = s.Id,
                MovieId = s.MovieId,
                MovieTitle = s.Movie?.Title ?? "",
                CinemaName = s.Hall?.Cinema?.Name ?? "",
                HallName = s.Hall?.Name ?? ""
            }).ToList();

            return View(viewModelList);
        }
    }
}