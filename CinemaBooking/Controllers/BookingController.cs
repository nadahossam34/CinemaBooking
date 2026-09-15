using BuisnessLogicLayer.Services;
using BuisnessLogicLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    public class BookingController : Controller
    {
        private readonly BookingService _bookingService;
        private readonly QRCodeService _qrCodeService;

        public BookingController(
            BookingService bookingService,
            QRCodeService qrCodeService)
        {
            _bookingService = bookingService;
            _qrCodeService = qrCodeService;
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