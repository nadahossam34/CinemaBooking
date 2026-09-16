using CinemaBooking.Data;
using CinemaBooking.Models;

namespace BuisnessLogicLayer.Services
{
    public class PaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public Payment ProcessPayment(
            int bookingId,
            decimal amount,
            string paymentMethod)
        {
            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = amount,
                Method = paymentMethod,
                Status = "Paid",
                TransactionReference = Guid.NewGuid().ToString("N"),
                PaidAt = DateTime.Now
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            return payment;
        }
    }
}