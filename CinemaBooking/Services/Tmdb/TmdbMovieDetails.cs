using System.Text.Json.Serialization;

namespace CinemaBooking.Services.Tmdb
{
    /// <summary>
    /// Only the fields that map onto the existing Movie entity
    /// (Title, Description, DurationMinutes, Rating, PosterUrl, ReleaseStatus, Genre, TmdbId)
    /// are captured here. TMDB returns many more fields; they are intentionally
    /// left out since the Movie entity has nowhere to put them (e.g. there is
    /// no ReleaseDate column on Movie today).
    /// </summary>
    public class TmdbMovieDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("runtime")]
        public int? Runtime { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("genres")]
        public List<TmdbGenre> Genres { get; set; } = new();
    }
}
