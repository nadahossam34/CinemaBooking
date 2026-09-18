using CinemaBooking.Services.Tmdb;

namespace CinemaBooking.Services
{
    /// <summary>
    /// Dedicated abstraction for talking to TMDB. Keeps all HTTP/API concerns
    /// out of AdminController/MoviesController.
    /// </summary>
    public interface ITmdbService
    {
        Task<TmdbResult<TmdbMovieSearchResponse>> SearchMoviesAsync(string query, CancellationToken cancellationToken = default);

        Task<TmdbResult<TmdbMovieDetails>> GetMovieDetailsAsync(int tmdbMovieId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Builds a full poster image URL from the partial path TMDB returns
        /// (e.g. "/abc.jpg" -> "https://image.tmdb.org/t/p/w500/abc.jpg").
        /// Returns null when there is no poster.
        /// </summary>
        string? BuildPosterUrl(string? posterPath);
    }
}
