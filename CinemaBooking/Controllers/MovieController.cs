using BuisnessLogicLayer.Service;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    public class MoviesController : Controller
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        public async Task<IActionResult> Index(string? search, string? genre)
        {
            var movies = await _movieService.GetAllMoviesAsync();
            ViewData["InitialSearch"] = search ?? "";
            ViewData["InitialGenre"] = genre ?? "";
            return View(movies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }
    }
}
