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

        [JsonPropertyName("original_title")]
        public string? OriginalTitle { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("runtime")]
        public int? Runtime { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("vote_count")]
        public int? VoteCount { get; set; }

        [JsonPropertyName("tagline")]
        public string? Tagline { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("genres")]
        public List<TmdbGenre> Genres { get; set; } = new();

        [JsonPropertyName("credits")]
        public TmdbCredits? Credits { get; set; }
    }

    public class TmdbCredits
    {
        [JsonPropertyName("crew")]
        public List<TmdbCrewMember> Crew { get; set; } = new();
    }

    public class TmdbCrewMember
    {
        [JsonPropertyName("job")]
        public string? Job { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
