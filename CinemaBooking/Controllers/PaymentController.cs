using BuisnessLogicLayer.Services;
using BuisnessLogicLayer.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public IActionResult Payment(int bookingId, decimal amount)
        {
            var model = new PaymentViewModel
            {
                BookingId = bookingId,
                Amount = amount
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Payment(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var payment = _paymentService.ProcessPayment(
                model.BookingId,
                model.Amount,
                model.PaymentMethod
            );

            return RedirectToAction("Ticket" , "Booking", new
            {
                bookingId = model.BookingId

            }
            );
        }
    }
}