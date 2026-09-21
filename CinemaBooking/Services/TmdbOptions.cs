namespace CinemaBooking.Services
{
    /// <summary>
    /// Bound from the "Tmdb" configuration section. ApiReadAccessToken must never be
    /// committed to source control - set it via `dotnet user-secrets` locally, or an
    /// environment variable / secret store (e.g. Tmdb__ApiReadAccessToken) in production.
    /// </summary>
    public class TmdbOptions
    {
        public const string SectionName = "Tmdb";

        public string BaseUrl { get; set; } = "https://api.themoviedb.org/3/";

        public string ApiReadAccessToken { get; set; } = string.Empty;

        public string ImageBaseUrl { get; set; } = "https://image.tmdb.org/t/p/";

        public string PosterSize { get; set; } = "w500";
    }
}
