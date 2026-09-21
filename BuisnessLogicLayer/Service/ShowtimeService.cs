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

        public async Task<IEnumerable<Showtime>> GetAllShowtimesAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var upcoming = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
                .Where(s => s.Date >= today)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.Time)
                .ToListAsync();

            if (upcoming.Any())
            {
                return upcoming;
            }

            // Fallback: if no upcoming showtimes scheduled, return all recent showtimes
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
                .OrderByDescending(s => s.Date)
                .ThenBy(s => s.Time)
                .Take(50)
                .ToListAsync();
        }

        public async Task<IEnumerable<Showtime>> GetShowtimesByMovieIdAsync(int movieId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var upcoming = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
                .Where(s => s.MovieId == movieId && s.Date >= today)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.Time)
                .ToListAsync();

            if (upcoming.Any())
            {
                return upcoming;
            }

            // Fallback to any showtimes for this movie
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
                .Where(s => s.MovieId == movieId)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.Time)
                .ToListAsync();
        }

        public async Task<Showtime?> GetShowtimeByIdAsync(int showtimeId)
        {
            return await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
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