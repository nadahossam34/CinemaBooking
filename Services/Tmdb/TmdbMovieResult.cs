using System.Text.Json.Serialization;

namespace CinemaBooking.Services.Tmdb
{
    /// <summary>
    /// A single item from TMDB's GET /search/movie response.
    /// Deliberately separate from the DataAccessLayer Movie entity - shapes differ
    /// (e.g. TMDB uses genre_ids here, not genre names) and this is never
    /// persisted directly.
    /// </summary>
    public class TmdbMovieResult
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }
    }
}
