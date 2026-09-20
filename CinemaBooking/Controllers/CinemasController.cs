using BuisnessLogicLayer.Service;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    public class CinemasController : Controller
    {
        private readonly ICinemaService _cinemaService;

        public CinemasController(ICinemaService cinemaService)
        {
            _cinemaService = cinemaService;
        }

        public async Task<IActionResult> Index()
        {
            var cinemas = await _cinemaService.GetAllCinemasAsync();

            return View(cinemas);
        }

        public async Task<IActionResult> Details(int id)
        {
            var cinema = await _cinemaService.GetCinemaByIdAsync(id);

            if (cinema == null)
                return NotFound();

            return View(cinema);
        }

        [HttpGet]
        public async Task<IActionResult> Nearest(
            double latitude,
            double longitude)
        {
            var cinema = await _cinemaService
                .GetNearestCinemaAsync(latitude, longitude);

            if (cinema == null)
                return NotFound();

            return View(cinema);
        }
    }
}
