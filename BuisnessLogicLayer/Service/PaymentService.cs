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
            var method = string.IsNullOrWhiteSpace(paymentMethod) ? "Card" : paymentMethod;
            var isCash = method.Equals("Cash", StringComparison.OrdinalIgnoreCase);
            var status = isCash ? "Pending (Pay at Box Office)" : "Paid";

            var existingPayment = _context.Payments.FirstOrDefault(p => p.BookingId == bookingId);
            if (existingPayment != null)
            {
                if (amount > 0)
                {
                    existingPayment.Amount = amount;
                }
                existingPayment.Method = method;
                existingPayment.Status = status;
                existingPayment.PaidAt = DateTime.Now;
                _context.SaveChanges();
                return existingPayment;
            }

            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = amount,
                Method = method,
                Status = status,
                TransactionReference = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper(),
                PaidAt = DateTime.Now
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            return payment;
        }
    }
}