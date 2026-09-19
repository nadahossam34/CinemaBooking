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
                .Include(b => b.BookingSeats)
                    .ThenInclude(bs => bs.Seat)
                .OrderByDescending(b => b.Showtime.Date)
                    .ThenByDescending(b => b.Showtime.Time)
                .ToListAsync();

            return View(bookings);
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var booking = _bookingService.CreateBooking(
                model.ShowtimeId,
                model.SelectedSeatIds,
                model.CustomerName,
                model.CustomerEmail
            );

            var qrData = $"Booking:{booking.Id}|Reference:{booking.BookingReference}";

            var qrCode = _qrCodeService.GenerateQRCode(qrData);

            return RedirectToAction(
                "Payment",
                "Payment",
                new
                {
                    bookingId = booking.Id,
                    amount = model.TotalAmount
                }
            );
        }

        [HttpGet]
        public IActionResult Ticket(int bookingId)
        {
            var booking = _bookingService.GetBooking(bookingId);

            if (booking == null)
                return NotFound();

            var qrData = $"Booking:{booking.Id}|Reference:{booking.BookingReference}";

            var qrCode = _qrCodeService.GenerateQRCode(qrData);

            var model = new TicketViewModel
            {
                BookingId = booking.Id,
                TicketNumber = booking.BookingReference,

                MovieTitle = booking.Showtime.Movie.Title,
                CinemaName = booking.Showtime.Cinema.Name,
                HallName = booking.Showtime.Hall.Name,

                Showtime = $"{booking.Showtime.Date} {booking.Showtime.Time}",

                Seats = string.Join(
                    ", ",
                    booking.BookingSeats.Select(
                        bs => $"{bs.Seat.RowLabel}{bs.Seat.SeatNumber}"
                    )
                ),

                TotalAmount = booking.BookingSeats.Sum(bs => bs.PricePaid),

                QRCodeImage =
                    $"data:image/png;base64,{Convert.ToBase64String(qrCode)}"
            };

            return View(model);
        }
    }
}