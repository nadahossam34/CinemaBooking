using CinemaBooking.Data;
using CinemaBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BuisnessLogicLayer.Services
{
    public class BookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public Booking CreateBooking(
            int showtimeId,
            List<int> seatIds,
            string customerName,
            string customerEmail,
            int? userId = null)
        {
            var showtime = _context.Showtimes.Find(showtimeId);

            if (showtime == null)
                throw new Exception("Showtime not found.");

            var booking = new Booking
            {
                UserId = userId,
                ShowtimeId = showtimeId,
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                BookingReference = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.Now,
                BookingDate = DateTime.Now,
                IsCheckedIn = false
            };

            foreach (var seatId in seatIds)
            {
                booking.BookingSeats.Add(new BookingSeat
                {
                    SeatId = seatId,
                    PricePaid = showtime.BasePrice
                });
            }

            _context.Bookings.Add(booking);
            _context.SaveChanges();

            return booking;
        }

        public Booking? GetBooking(int bookingId)
        {
            return _context.Bookings
              .Include(b => b.Showtime)
                 .ThenInclude(s => s.Movie)
              .Include(b => b.Showtime)
                 .ThenInclude(s => s.Cinema)
              .Include(b => b.Showtime)
                 .ThenInclude(s => s.Hall)
              .Include(b => b.BookingSeats)
                 .ThenInclude(bs => bs.Seat)
              .FirstOrDefault(b => b.Id == bookingId);
        }
    }
}