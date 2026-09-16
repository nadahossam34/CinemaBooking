using System.Net;
using System.Net.Http.Json;
using CinemaBooking.Services.Tmdb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CinemaBooking.Services
{
    /// <summary>
    /// Talks to TMDB (https://developer.themoviedb.org/docs/getting-started) using
    /// a typed HttpClient registered via IHttpClientFactory (AddHttpClient in Program.cs).
    /// The API credential is attached as a Bearer token in Program.cs when the client
    /// is configured - it is never read or referenced here from a hard-coded value.
    /// </summary>
    public class TmdbService : ITmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly TmdbOptions _options;
        private readonly ILogger<TmdbService> _logger;

        public TmdbService(HttpClient httpClient, IOptions<TmdbOptions> options, ILogger<TmdbService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<TmdbResult<TmdbMovieSearchResponse>> SearchMoviesAsync(string query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return TmdbResult<TmdbMovieSearchResponse>.Fail("Please enter a movie title to search for.");
            }

            if (string.IsNullOrWhiteSpace(_options.ApiReadAccessToken))
            {
                _logger.LogError("TMDB API read access token is not configured.");
                return TmdbResult<TmdbMovieSearchResponse>.Fail("TMDB is not configured yet. Ask an administrator to set the API credential.");
            }

            var requestUri = $"search/movie?query={Uri.EscapeDataString(query)}&include_adult=false&language=en-US&page=1";

            try
            {
                using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("TMDB rejected the configured API credential (401 Unauthorized).");
                    return TmdbResult<TmdbMovieSearchResponse>.Fail("TMDB rejected the configured API credential.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("TMDB search request failed with status {StatusCode}.", response.StatusCode);
                    return TmdbResult<TmdbMovieSearchResponse>.Fail("TMDB could not be reached right now. Please try again shortly.");
                }

                var payload = await response.Content.ReadFromJsonAsync<TmdbMovieSearchResponse>(cancellationToken: cancellationToken);

                if (payload == null)
                {
                    _logger.LogError("TMDB search returned an empty/unparseable response body.");
                    return TmdbResult<TmdbMovieSearchResponse>.Fail("TMDB returned an unexpected response.");
                }

                return TmdbResult<TmdbMovieSearchResponse>.Ok(payload);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError("TMDB search request timed out.");
                return TmdbResult<TmdbMovieSearchResponse>.Fail("The request to TMDB timed out. Please try again.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while contacting TMDB (search).");
                return TmdbResult<TmdbMovieSearchResponse>.Fail("Could not reach TMDB. Please check your connection and try again.");
            }
        }

        public async Task<TmdbResult<TmdbMovieDetails>> GetMovieDetailsAsync(int tmdbMovieId, CancellationToken cancellationToken = default)
        {
            if (tmdbMovieId <= 0)
            {
                return TmdbResult<TmdbMovieDetails>.Fail("That TMDB movie id is not valid.");
            }

            if (string.IsNullOrWhiteSpace(_options.ApiReadAccessToken))
            {
                _logger.LogError("TMDB API read access token is not configured.");
                return TmdbResult<TmdbMovieDetails>.Fail("TMDB is not configured yet. Ask an administrator to set the API credential.");
            }

            try
            {
                using var response = await _httpClient.GetAsync($"movie/{tmdbMovieId}?language=en-US", cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return TmdbResult<TmdbMovieDetails>.Fail("That movie could not be found on TMDB.");
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("TMDB rejected the configured API credential (401 Unauthorized).");
                    return TmdbResult<TmdbMovieDetails>.Fail("TMDB rejected the configured API credential.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("TMDB movie details request failed with status {StatusCode}.", response.StatusCode);
                    return TmdbResult<TmdbMovieDetails>.Fail("TMDB could not be reached right now. Please try again shortly.");
                }

                var payload = await response.Content.ReadFromJsonAsync<TmdbMovieDetails>(cancellationToken: cancellationToken);

                if (payload == null)
                {
                    _logger.LogError("TMDB movie details returned an empty/unparseable response body.");
                    return TmdbResult<TmdbMovieDetails>.Fail("TMDB returned an unexpected response.");
                }

                return TmdbResult<TmdbMovieDetails>.Ok(payload);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError("TMDB movie details request timed out.");
                return TmdbResult<TmdbMovieDetails>.Fail("The request to TMDB timed out. Please try again.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while contacting TMDB (movie details).");
                return TmdbResult<TmdbMovieDetails>.Fail("Could not reach TMDB. Please check your connection and try again.");
            }
        }

        public string? BuildPosterUrl(string? posterPath)
        {
            if (string.IsNullOrWhiteSpace(posterPath))
            {
                return null;
            }

            var baseUrl = _options.ImageBaseUrl.TrimEnd('/');
            var size = _options.PosterSize.Trim('/');
            var path = posterPath.StartsWith('/') ? posterPath : "/" + posterPath;

            return $"{baseUrl}/{size}{path}";
        }
    }
}
