using DataAccessLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CinemaBooking.Data;
using Microsoft.EntityFrameworkCore;
namespace BuisnessLogicLayer.Service
{
    public class MovieService : IMovieService
    {
        private readonly AppDbContext _context;

        public MovieService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MovieViewModel>> GetAllMoviesAsync()
        {
            // Materialize first: Year is parsed from the ReleaseDate string, which
            // isn't translatable to SQL, so the mapping happens client-side.
            var movies = await _context.Movies
                .AsNoTracking()
                .ToListAsync();

            return movies.Select(MapToViewModel).ToList();
        }

        public async Task<MovieDetailsViewModel?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies
                .AsNoTracking()
                .Where(m => m.Id == id)
                .Select(m => new MovieDetailsViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    Genre = m.Genre,
                    DurationMinutes = m.DurationMinutes,
                    Description = m.Description,
                    Rating = m.Rating,
                    PosterUrl = m.PosterUrl,
                    TrailerUrl = m.TrailerUrl,
                    ReleaseStatus = m.ReleaseStatus,
                    TmdbId = m.TmdbId,

                    Cinemas = m.Showtimes
                        .Select(s => s.Cinema)
                        .Distinct()
                        .Select(c => new CinemaViewModel
                        {
                            Id = c.Id,
                            Name = c.Name,
                            City = c.City,
                            Address = c.Address,
                            Latitude = c.Latitude,
                            Longitude = c.Longitude
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        private static MovieViewModel MapToViewModel(Movie m)
        {
            return new MovieViewModel
            {
                Id = m.Id,
                Title = m.Title,
                Genre = m.Genre,
                DurationMinutes = m.DurationMinutes,
                Rating = m.Rating,
                PosterUrl = m.PosterUrl,
                ReleaseStatus = m.ReleaseStatus,
                Description = m.Description,
                Format = m.Formats,
                Year = TryParseYear(m.ReleaseDate),
                Score = m.VoteAverage,
                AgeRating = !string.IsNullOrWhiteSpace(m.Certification) ? m.Certification : m.Rating
            };
        }

        private static int TryParseYear(string? releaseDate)
        {
            if (!string.IsNullOrWhiteSpace(releaseDate)
                && releaseDate.Length >= 4
                && int.TryParse(releaseDate.Substring(0, 4), out var year))
            {
                return year;
            }

            return 0;
        }
    }
}
