using BuisnessLogicLayer.Service;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.ViewModels;

namespace PresentationLayer.Controllers
{
    public class SeatsController : Controller
    {
        private readonly IShowtimeService _showtimeService;

        public SeatsController(IShowtimeService showtimeService)
        {
            _showtimeService = showtimeService;
        }

        [HttpGet]
        public async Task<IActionResult> SelectSeats(int showtimeId)
        {
            var showtime = await _showtimeService.GetShowtimeByIdAsync(showtimeId);
            if (showtime == null)
            {
                return NotFound();
            }

            var allSeats = await _showtimeService.GetSeatsByHallIdAsync(showtime.HallId);
            var bookedSeatIds = await _showtimeService.GetBookedSeatIdsAsync(showtimeId);

            var seatViewModels = allSeats.Select(s => new SeatViewModel
            {
                SeatId = s.Id,
                SeatNumber = s.SeatNumber.ToString(),
                SeatTypeName = s.SeatType?.Name ?? "Standard",
                IsBooked = bookedSeatIds.Contains(s.Id),
                IsSelected = false
            }).ToList();

            var viewModel = new SeatMapViewModel
            {
                ShowtimeId = showtime.Id,
                MovieTitle = showtime.Movie?.Title ?? "",
                CinemaName = showtime.Hall?.Cinema?.Name ?? "",
                HallName = showtime.Hall?.Name ?? "",
                Seats = seatViewModels
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ConfirmSeats(BookingSeatViewModel model)
        {
            if (model.SelectedSeatIds == null || !model.SelectedSeatIds.Any())
            {
                ModelState.AddModelError("", "Please select at least one seat.");
                return RedirectToAction("SelectSeats", new { showtimeId = model.ShowtimeId });
            }

            return RedirectToAction("Index", "Home");
        }
    }
}