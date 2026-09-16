using CinemaBooking.Data;
using CinemaBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BuisnessLogicLayer.Service
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly AppDbContext _context;

        public ShowtimeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Showtime>> GetShowtimesByMovieIdAsync(int movieId)
        {
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .ThenInclude(h => h.Cinema)
                .Where(s => s.MovieId == movieId)
                .ToListAsync();
        }

        public async Task<Showtime?> GetShowtimeByIdAsync(int showtimeId)
        {
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .ThenInclude(h => h.Cinema)
                .FirstOrDefaultAsync(s => s.Id == showtimeId);
        }

        public async Task<IEnumerable<Seat>> GetSeatsByHallIdAsync(int hallId)
        {
            return await _context.Seats
                .Include(s => s.SeatType)
                .Where(s => s.HallId == hallId)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetBookedSeatIdsAsync(int showtimeId)
        {
            return await _context.Bookings
                .Where(b => b.ShowtimeId == showtimeId)
                .SelectMany(b => b.BookingSeats)
                .Select(bs => bs.SeatId)
                .ToListAsync();
        }
    }
}