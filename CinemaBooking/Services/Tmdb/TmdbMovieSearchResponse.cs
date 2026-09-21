using System.Text.Json.Serialization;

namespace CinemaBooking.Services.Tmdb
{
    public class TmdbMovieSearchResponse
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("results")]
        public List<TmdbMovieResult> Results { get; set; } = new();

        [JsonPropertyName("total_results")]
        public int TotalResults { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }
    }
}
