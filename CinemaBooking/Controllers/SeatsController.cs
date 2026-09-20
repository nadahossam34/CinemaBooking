using BuisnessLogicLayer.Service;
using CinemaBooking.Data;
using CinemaBooking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresentationLayer.ViewModels;

namespace PresentationLayer.Controllers
{
    public class SeatsController : Controller
    {
        private readonly IShowtimeService _showtimeService;
        private readonly AppDbContext _context;

        public SeatsController(IShowtimeService showtimeService, AppDbContext context)
        {
            _showtimeService = showtimeService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> SelectSeats(int showtimeId)
        {
            var showtime = await _showtimeService.GetShowtimeByIdAsync(showtimeId);
            if (showtime == null)
            {
                return NotFound();
            }

            var allSeats = (await _showtimeService.GetSeatsByHallIdAsync(showtime.HallId)).ToList();

            if (!allSeats.Any())
            {
                // Auto-populate seats for this hall if missing
                var standardType = await _context.SeatTypes.FirstOrDefaultAsync(st => st.Name == "Standard");
                var vipType = await _context.SeatTypes.FirstOrDefaultAsync(st => st.Name == "VIP") ?? standardType;

                if (standardType != null)
                {
                    var newSeats = new List<Seat>();
                    var rows = new[] { "A", "B", "C", "D", "E" };
                    foreach (var row in rows)
                    {
                        for (int num = 1; num <= 8; num++)
                        {
                            newSeats.Add(new Seat
                            {
                                HallId = showtime.HallId,
                                RowLabel = row,
                                SeatNumber = num,
                                SeatTypeId = (row == "E") ? (vipType?.Id ?? standardType.Id) : standardType.Id
                            });
                        }
                    }
                    _context.Seats.AddRange(newSeats);
                    await _context.SaveChangesAsync();

                    allSeats = (await _showtimeService.GetSeatsByHallIdAsync(showtime.HallId)).ToList();
                }
            }

            var bookedSeatIds = await _showtimeService.GetBookedSeatIdsAsync(showtimeId);

            var seatViewModels = allSeats.Select(s => new SeatViewModel
            {
                SeatId = s.Id,
                SeatNumber = $"{s.RowLabel}{s.SeatNumber}",
                RowLabel = s.RowLabel ?? "A",
                ColumnNumber = s.SeatNumber,
                SeatTypeName = s.SeatType?.Name ?? "Standard",
                Price = showtime.BasePrice * (s.SeatType?.PriceMultiplier ?? 1.0m),
                IsBooked = bookedSeatIds.Contains(s.Id),
                IsSelected = false
            }).OrderBy(s => s.RowLabel).ThenBy(s => s.ColumnNumber).ToList();

            var viewModel = new SeatMapViewModel
            {
                ShowtimeId = showtime.Id,
                MovieTitle = showtime.Movie?.Title ?? "Movie Feature",
                CinemaName = showtime.Cinema?.Name ?? showtime.Hall?.Cinema?.Name ?? "StarLight Cinema",
                HallName = showtime.Hall?.Name ?? "Auditorium",
                StartTime = showtime.Date.ToDateTime(showtime.Time),
                BasePrice = showtime.BasePrice,
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

            return RedirectToAction("Checkout", "Booking", new
            {
                showtimeId = model.ShowtimeId,
                seats = string.Join(",", model.SelectedSeatIds)
            });
        }
    }
}