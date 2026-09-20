using BuisnessLogicLayer.Services;
using BuisnessLogicLayer.ViewModels;
using CinemaBooking.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentService _paymentService;
        private readonly AppDbContext _context;

        public PaymentController(PaymentService paymentService, AppDbContext context)
        {
            _paymentService = paymentService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Payment(int bookingId, decimal amount)
        {
            if (bookingId <= 0)
            {
                return RedirectToAction("Index", "Movies");
            }

            var booking = await _context.Bookings
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Cinema)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Hall)
                .Include(b => b.BookingSeats)
                    .ThenInclude(bs => bs.Seat)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            var calculatedAmount = booking.BookingSeats.Sum(bs => bs.PricePaid);
            var payableAmount = (amount > 0) ? amount : (calculatedAmount > 0 ? calculatedAmount : 180.00m);

            ViewBag.Booking = booking;

            var model = new PaymentViewModel
            {
                BookingId = bookingId,
                Amount = payableAmount,
                PaymentMethod = "Card",
                CardNumber = "4111222233334444",
                ExpiryDate = "12/28",
                CVV = "789"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(PaymentViewModel model)
        {
            if (model.BookingId <= 0)
            {
                return RedirectToAction("Index", "Movies");
            }

            var method = string.IsNullOrWhiteSpace(model.PaymentMethod) ? "Card" : model.PaymentMethod;

            if (model.Amount <= 0)
            {
                var booking = await _context.Bookings
                    .Include(b => b.BookingSeats)
                    .FirstOrDefaultAsync(b => b.Id == model.BookingId);

                if (booking != null && booking.BookingSeats.Any())
                {
                    model.Amount = booking.BookingSeats.Sum(bs => bs.PricePaid);
                }
                else
                {
                    model.Amount = 180.00m;
                }
            }

            _paymentService.ProcessPayment(
                model.BookingId,
                model.Amount,
                method
            );

            return RedirectToAction("Ticket", "Booking", new
            {
                bookingId = model.BookingId
            });
        }
    }
}