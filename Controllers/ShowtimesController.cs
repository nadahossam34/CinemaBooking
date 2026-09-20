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

        public async Task<IActionResult> Index(int? movieId = null)
        {
            var showtimes = (movieId.HasValue && movieId.Value > 0)
                ? await _showtimeService.GetShowtimesByMovieIdAsync(movieId.Value)
                : await _showtimeService.GetAllShowtimesAsync();

            ViewBag.SelectedMovieId = movieId;

            var viewModelList = showtimes.Select(s => new ShowtimeViewModel
            {
                ShowtimeId = s.Id,
                MovieId = s.MovieId,
                MovieTitle = s.Movie?.Title ?? "Movie",
                MoviePosterUrl = s.Movie?.PosterUrl ?? "",
                CinemaId = s.CinemaId,
                CinemaName = s.Cinema?.Name ?? s.Hall?.Cinema?.Name ?? "StarLight Cinema",
                HallName = s.Hall?.Name ?? "Auditorium",
                StartTime = s.Date.ToDateTime(s.Time),
                TicketPrice = s.BasePrice
            }).ToList();

            return View(viewModelList);
        }
    }
}