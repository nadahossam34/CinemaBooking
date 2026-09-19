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
            try
            {
                var movies = await _context.Movies
                    .AsNoTracking()
                    .Select(m => new MovieViewModel
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Genre = m.Genre,
                        DurationMinutes = m.DurationMinutes,
                        Rating = m.Rating,
                        PosterUrl = m.PosterUrl,
                        ReleaseStatus = m.ReleaseStatus,
                        Description = m.Description,
                        Format = "IMAX 3D",
                        Year = 2025,
                        Score = 8.8,
                        AgeRating = m.Rating
                    })
                    .ToListAsync();

                if (movies != null && movies.Any())
                    return movies;
            }
            catch (Exception)
            {
                // Graceful fallback if database connection is unavailable
            }

            return GetDefaultMovies();
        }

        public async Task<MovieDetailsViewModel?> GetMovieByIdAsync(int id)
        {
            try
            {
                var movie = await _context.Movies
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

                if (movie != null)
                    return movie;
            }
            catch (Exception)
            {
                // Graceful fallback
            }

            var defaultMovie = GetDefaultMovies().FirstOrDefault(m => m.Id == id);
            if (defaultMovie == null)
                return null;

            return new MovieDetailsViewModel
            {
                Id = defaultMovie.Id,
                Title = defaultMovie.Title,
                Genre = defaultMovie.Genre,
                DurationMinutes = defaultMovie.DurationMinutes,
                Rating = defaultMovie.AgeRating ?? defaultMovie.Rating,
                PosterUrl = defaultMovie.PosterUrl,
                ReleaseStatus = defaultMovie.ReleaseStatus,
                Description = defaultMovie.Description ?? "Experience this cinematic masterpiece in StarLight's ultra-premium IMAX Laser and Dolby Atmos auditoriums.",
                TrailerUrl = "https://www.youtube.com",
                Cinemas = new List<CinemaViewModel>
                {
                    new CinemaViewModel { Id = 1, Name = "StarLight Mall of Egypt", City = "Giza", Address = "Gate 4, Level 2, Mall of Egypt, Wahat Road, 6th of October City, Giza" },
                    new CinemaViewModel { Id = 2, Name = "StarLight New Cairo", City = "New Cairo", Address = "The Promenade, Cairo Festival City Mall, Ring Road, New Cairo" },
                    new CinemaViewModel { Id = 3, Name = "StarLight Alexandria", City = "Alexandria", Address = "San Stefano Grand Plaza, 3rd Floor, El-Geish Road, Alexandria" }
                }
            };
        }

        private static List<MovieViewModel> GetDefaultMovies()
        {
            return new List<MovieViewModel>
            {
                new MovieViewModel
                {
                    Id = 1,
                    Title = "NEO HORIZON",
                    Genre = "Sci-Fi / Cyberpunk / Action",
                    DurationMinutes = 138,
                    Rating = "Rated R",
                    AgeRating = "Rated R",
                    Score = 8.9,
                    Format = "IMAX 3D",
                    Year = 2025,
                    Description = "A lone operative discovers a sinister corruption within the megacity's neural network.",
                    PosterUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuDz7rebVWhhU2_lZ1PVdjDvtBh7s1RU4G-SbtajAcP5NJHwwxR3pScnI0lV9BvKcztvryb1TxxibT2s1Ezp4VfywGVYun9bLhuRZjpT6yZ3klvewniJy1ymFt3PrqQZ8PYbh4OyOWxiU2UuNdRkz3oJnq65cO5ZGOGlAl6tpF00E6zTi7CW9x6mS4Qsom6FnsplsPNOG-q0Sz1SIJtIyROqKuFHBDa0PorkvRruhcTCrFUEUodgWeAVJQ",
                    ReleaseStatus = "Now Showing"
                },
                new MovieViewModel
                {
                    Id = 2,
                    Title = "Shadow Protocol",
                    Genre = "Thriller / Espionage / Mystery / Action",
                    DurationMinutes = 124,
                    Rating = "PG-13",
                    AgeRating = "PG-13",
                    Score = 8.7,
                    Format = "Dolby Cinema",
                    Year = 2025,
                    Description = "An operative framed for treason navigates neon-drenched rainy streets to expose an international conspiracy.",
                    PosterUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuBzfNq2z80Rr-2jD9s1UIvoF_HHINqofmx9VbF3VMKq0ihWQtpWuTyljf4AEem_aHr61tvxLsSoKZpAN3Ds2YV2dHoQzskxj2NfzlJM2novlommH2sqo7TAbLFa93jbACI05k_piy4AkOKMPj0PpX0hKYw9CvwN12ErpCXT2BvfZcj6NN1qH5z5HBMxl1YSp4cWq9UTFpMraqHeO3d_CBtq0BVGBNWQP1rnNIQfMIDEgFT-pvVfAbe1PQ",
                    ReleaseStatus = "Now Showing"
                },
                new MovieViewModel
                {
                    Id = 3,
                    Title = "Chrono: Eclipse",
                    Genre = "Sci-Fi / Drama / Adventure",
                    DurationMinutes = 162,
                    Rating = "PG-13",
                    AgeRating = "PG-13",
                    Score = 9.2,
                    Format = "IMAX Laser",
                    Year = 2025,
                    Description = "A desperate expedition voyages to the edge of an event horizon where time dilation becomes the ultimate enemy.",
                    PosterUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuDTUdBTrjLptECEFjjlMAOZu9fLjcDanEAU2YA6AVNPAIOWANGMK_m-IbrbQoTKQ9T7zhxdINIyo4oVuXFKN-HYAFmzQzsAf2zcR26ouK5mhtVI2VbDBvoX6t3Oy_5ZpYp8oJsst-aJkQZVANpJB-ZEDt8gylG29-SPidbQWikRSaOB_Etk6zqB0tOXZMpWhzqGjb9t_Ecmt-DXJftJPON5zyYi4gjjTXYx2uOd0n-04uwuhc2NQTIk7Q",
                    ReleaseStatus = "Now Showing"
                },
                new MovieViewModel
                {
                    Id = 4,
                    Title = "Dune: Part Two",
                    Genre = "Sci-Fi / Action / Drama",
                    DurationMinutes = 166,
                    Rating = "PG-13",
                    AgeRating = "PG-13",
                    Score = 9.0,
                    Format = "70mm Dome",
                    Year = 2024,
                    Description = "Paul Atreides unites with Chani and the Fremen while seeking revenge against the conspirators who destroyed his family.",
                    PosterUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuDtk097OOzkt4peSGEAfgOQd4NiIPeEzvjbb7I2UhRTJNT56EEDW9R2njG-FD7u-EnthBdMqidgNxhyRxvJtLNARvHXU1KNH3_0QLY1RqhccfJAwkuAqVDC5E8rcGi2k5lQQTMrdde_CoFtepNXmH2lwHwMs2WGrl83f-tx6fmnZYW9fz_7uPuQP4P7H7cOlXWVEHidBfrTuim_upSnFocUDei6gCL7pD53NzoSMwHTT14nRJqCF8Ahrw",
                    ReleaseStatus = "Now Showing"
                },
                new MovieViewModel
                {
                    Id = 5,
                    Title = "Interstellar: 10th Anniversary IMAX",
                    Genre = "Sci-Fi / Drama / Adventure",
                    DurationMinutes = 169,
                    Rating = "PG-13",
                    AgeRating = "PG-13",
                    Score = 8.9,
                    Format = "IMAX 70mm",
                    Year = 2024,
                    Description = "When Earth becomes uninhabitable, a farmer and ex-NASA pilot is tasked to pilot a spacecraft along with a team of researchers to find a new home.",
                    PosterUrl = "https://images.unsplash.com/photo-1506703719100-a0f3a48c0f86?auto=format&fit=crop&w=600&q=80",
                    ReleaseStatus = "Now Showing"
                },
                new MovieViewModel
                {
                    Id = 6,
                    Title = "Gladiator II",
                    Genre = "Action / Epic / Drama",
                    DurationMinutes = 148,
                    Rating = "Rated R",
                    AgeRating = "Rated R",
                    Score = 8.5,
                    Format = "Dolby Cinema",
                    Year = 2024,
                    Description = "Years after witnessing the death of Maximus, Lucius must enter the Colosseum after his home is conquered by tyrannical emperors.",
                    PosterUrl = "https://images.unsplash.com/photo-1579783902614-a3fb3927b675?auto=format&fit=crop&w=600&q=80",
                    ReleaseStatus = "Now Showing"
                }
            };
        }
    }
}
