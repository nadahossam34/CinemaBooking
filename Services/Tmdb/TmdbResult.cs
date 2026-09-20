namespace CinemaBooking.Services.Tmdb
{
    /// <summary>
    /// Wraps a TMDB service call outcome so controllers can display a useful
    /// error message instead of the request failing/crashing when TMDB is
    /// unavailable, misconfigured, or returns an error.
    /// </summary>
    public class TmdbResult<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? ErrorMessage { get; init; }

        public static TmdbResult<T> Ok(T data) => new() { Success = true, Data = data };

        public static TmdbResult<T> Fail(string message) => new() { Success = false, ErrorMessage = message };
    }
}
