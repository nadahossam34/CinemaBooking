using CinemaBooking.Models;

namespace BuisnessLogicLayer.Service
{
    public interface IShowtimeService
    {
        Task<IEnumerable<Showtime>> GetAllShowtimesAsync();
        Task<IEnumerable<Showtime>> GetShowtimesByMovieIdAsync(int movieId);
        Task<Showtime?> GetShowtimeByIdAsync(int showtimeId);
        Task<IEnumerable<Seat>> GetSeatsByHallIdAsync(int hallId);
        Task<IEnumerable<int>> GetBookedSeatIdsAsync(int showtimeId);
    }
}