using System.Security.Claims;
using BuisnessLogicLayer.Services;
using BuisnessLogicLayer.ViewModels;
using CinemaBooking.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer.Controllers
{
    public class BookingController : Controller
    {
        private readonly BookingService _bookingService;
        private readonly QRCodeService _qrCodeService;
        private readonly AppDbContext _context;

        public BookingController(
            BookingService bookingService,
            QRCodeService qrCodeService,
            AppDbContext context)
        {
            _bookingService = bookingService;
            _qrCodeService = qrCodeService;
            _context = context;
        }

        // GET: /Booking/MyBookings
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = await _context.Bookings
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Cinema)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Hall)
                .Include(b => b.BookingSeats)
                    .ThenInclude(bs => bs.Seat)
                .Include(b => b.Payment)
                .OrderByDescending(b => b.Showtime.Date)
                    .ThenByDescending(b => b.Showtime.Time)
                .ToListAsync();

            return View(bookings);
        }

        [HttpGet]
        public IActionResult Checkout(int? showtimeId, string? seats)
        {
            if (!showtimeId.HasValue)
            {
                return View(new CheckoutViewModel());
            }

            var showtime = _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
                .FirstOrDefault(s => s.Id == showtimeId.Value);

            if (showtime == null)
            {
                return NotFound();
            }

            var seatIds = (seats ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            var seatList = _context.Seats
                .Include(s => s.SeatType)
                .Where(s => seatIds.Contains(s.Id))
                .ToList();

            var seatLabels = seatList
                .Select(s => $"{s.RowLabel}{s.SeatNumber}")
                .ToList();

            var totalPrice = seatList.Any()
                ? seatList.Sum(s => showtime.BasePrice * (s.SeatType?.PriceMultiplier ?? 1.0m))
                : seatIds.Count * showtime.BasePrice;

            var model = new CheckoutViewModel
            {
                ShowtimeId = showtime.Id,
                MovieTitle = showtime.Movie?.Title ?? "Movie",
                CinemaName = showtime.Cinema?.Name ?? showtime.Hall?.Cinema?.Name ?? "StarLight Cinema",
                HallName = showtime.Hall?.Name ?? "Auditorium",
                Showtime = $"{showtime.Date:dddd, MMM d yyyy} · {showtime.Time:h:mm tt}",
                SelectedSeatIds = seatIds,
                TotalAmount = totalPrice,
                CustomerName = User.FindFirstValue(ClaimTypes.Name) ?? "",
                CustomerEmail = User.FindFirstValue(ClaimTypes.Email) ?? ""
            };

            ViewData["PosterUrl"] = showtime.Movie?.PosterUrl;
            ViewData["SeatLabels"] = string.Join(", ", seatLabels);
            ViewData["PricePerSeat"] = showtime.BasePrice;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            // Clear any validation issues for display-only fields
            ModelState.Remove(nameof(model.MovieTitle));
            ModelState.Remove(nameof(model.CinemaName));
            ModelState.Remove(nameof(model.HallName));
            ModelState.Remove(nameof(model.Showtime));

            if (string.IsNullOrWhiteSpace(model.CustomerName))
            {
                ModelState.AddModelError(nameof(model.CustomerName), "Please enter your full name.");
            }
            if (string.IsNullOrWhiteSpace(model.CustomerEmail))
            {
                ModelState.AddModelError(nameof(model.CustomerEmail), "Please enter your email address.");
            }

            if (!ModelState.IsValid || model.SelectedSeatIds == null || !model.SelectedSeatIds.Any())
            {
                if (model.SelectedSeatIds == null || !model.SelectedSeatIds.Any())
                {
                    ModelState.AddModelError(string.Empty, "No seats were selected. Please select your seats.");
                }

                // Re-hydrate view info
                var st = _context.Showtimes
                    .Include(s => s.Movie)
                    .Include(s => s.Cinema)
                    .Include(s => s.Hall)
                        .ThenInclude(h => h.Cinema)
                    .FirstOrDefault(s => s.Id == model.ShowtimeId);

                if (st != null)
                {
                    model.MovieTitle = st.Movie?.Title ?? "Movie";
                    model.CinemaName = st.Cinema?.Name ?? st.Hall?.Cinema?.Name ?? "StarLight Cinema";
                    model.HallName = st.Hall?.Name ?? "Auditorium";
                    model.Showtime = $"{st.Date:dddd, MMM d yyyy} · {st.Time:h:mm tt}";

                    var seats = _context.Seats
                        .Where(s => model.SelectedSeatIds != null && model.SelectedSeatIds.Contains(s.Id))
                        .Select(s => $"{s.RowLabel}{s.SeatNumber}")
                        .ToList();

                    ViewData["PosterUrl"] = st.Movie?.PosterUrl;
                    ViewData["SeatLabels"] = string.Join(", ", seats);
                    ViewData["PricePerSeat"] = st.BasePrice;
                }

                return View(model);
            }

            int? userId = null;
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdValue, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var booking = _bookingService.CreateBooking(
                model.ShowtimeId,
                model.SelectedSeatIds,
                model.CustomerName.Trim(),
                model.CustomerEmail.Trim(),
                userId
            );

            var realAmount = booking.BookingSeats.Sum(bs => bs.PricePaid);
            if (realAmount <= 0)
            {
                realAmount = model.TotalAmount > 0 ? model.TotalAmount : 180.00m;
            }

            return RedirectToAction(
                "Payment",
                "Payment",
                new
                {
                    bookingId = booking.Id,
                    amount = realAmount
                }
            );
        }

        [HttpGet]
        public IActionResult Ticket(int bookingId)
        {
            var booking = _bookingService.GetBooking(bookingId);

            if (booking == null)
                return NotFound();

            var qrData = $"STARLIGHT|Booking:{booking.Id}|Ref:{booking.BookingReference}";
            var qrCode = _qrCodeService.GenerateQRCode(qrData);

            var movieTitle = booking.Showtime?.Movie?.Title ?? "Feature Presentation";
            var cinemaName = booking.Showtime?.Cinema?.Name ?? booking.Showtime?.Hall?.Cinema?.Name ?? "StarLight Multiplex";
            var hallName = booking.Showtime?.Hall?.Name ?? "Main Hall";
            var showtimeStr = booking.Showtime != null
                ? $"{booking.Showtime.Date:dddd, MMM d yyyy} · {booking.Showtime.Time:h:mm tt}"
                : "Scheduled Screening";

            var seatLabels = booking.BookingSeats != null && booking.BookingSeats.Any()
                ? string.Join(", ", booking.BookingSeats.Select(bs => bs.Seat != null ? $"{bs.Seat.RowLabel}{bs.Seat.SeatNumber}" : $"Seat #{bs.SeatId}"))
                : "Reserved";

            var payment = _context.Payments.FirstOrDefault(p => p.BookingId == bookingId);
            var totalAmount = payment?.Amount ?? booking.BookingSeats?.Sum(bs => bs.PricePaid) ?? 0m;

            var model = new TicketViewModel
            {
                BookingId = booking.Id,
                TicketNumber = booking.BookingReference,
                MovieTitle = movieTitle,
                CinemaName = cinemaName,
                HallName = hallName,
                Showtime = showtimeStr,
                Seats = seatLabels,
                TotalAmount = totalAmount,
                QRCodeImage = $"data:image/png;base64,{Convert.ToBase64String(qrCode)}"
            };

            return View(model);
        }
    }
}