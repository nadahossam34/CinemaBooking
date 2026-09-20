using CinemaBooking.Data;
using CinemaBooking.Services;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers
{
    // Admin-only Movie CRUD + TMDB integration. Views live under Views/Admin/Movies/
    // (explicit paths below).
    //
    // Named "AdminMovies" rather than "Movies" because the public, service-based
    // MoviesController (PresentationLayer.Controllers, in Controllers/MovieController.cs)
    // already owns the "Movies" controller name. Two controllers with the same name
    // compile fine in different namespaces but make MVC throw AmbiguousMatchException
    // at request time, so the admin surface lives at /AdminMovies instead.
    [Authorize(Roles = "Admin")]
    public class AdminMoviesController : Controller
    {
        private const string ViewsFolder = "~/Views/Admin/Movies/";

        private readonly AppDbContext _context;
        private readonly ITmdbService _tmdbService;

        public AdminMoviesController(AppDbContext context, ITmdbService tmdbService)
        {
            _context = context;
            _tmdbService = tmdbService;
        }

        // GET: /AdminMovies
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .AsNoTracking()
                .Include(m => m.Showtimes)
                    .ThenInclude(s => s.Cinema)
                .OrderByDescending(m => m.Id)
                .ToListAsync();

            var totalCatalog = movies.Count;
            var nowShowingCount = movies.Count(m =>
                string.Equals(m.ReleaseStatus, "Now Showing", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.ReleaseStatus, "NowShowing", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.ReleaseStatus, "now_showing", StringComparison.OrdinalIgnoreCase));

            var vm = new MoviesIndexViewModel
            {
                Movies = movies,
                TotalCatalog = totalCatalog,
                NowShowingCount = nowShowingCount,
                IsTmdbConnected = _tmdbService.IsConfigured
            };

            return View(ViewsFolder + "Index.cshtml", vm);
        }

        // GET: /AdminMovies/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var cinemas = await _context.Cinemas.AsNoTracking().Include(c => c.Halls).OrderBy(c => c.Name).ToListAsync();
            var vm = new MovieFormViewModel
            {
                AvailableCinemas = cinemas,
                SelectedCinemaIds = cinemas.Take(3).Select(c => c.Id).ToList()
            };
            return View(ViewsFolder + "Create.cshtml", vm);
        }

        // POST: /AdminMovies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableCinemas = await _context.Cinemas.AsNoTracking().Include(c => c.Halls).OrderBy(c => c.Name).ToListAsync();
                return View(ViewsFolder + "Create.cshtml", model);
            }

            // Real Cinemas selected by Admin
            var selectedCinemas = await _context.Cinemas
                .Include(c => c.Halls)
                .Where(c => model.SelectedCinemaIds.Contains(c.Id))
                .ToListAsync();

            var locationNames = selectedCinemas.Select(c => c.Name).ToList();
            if (locationNames.Any())
            {
                model.Locations = string.Join(", ", locationNames);
            }

            var movie = new Movie
            {
                Title = model.Title.Trim(),
                Genre = model.Genre?.Trim() ?? string.Empty,
                DurationMinutes = model.DurationMinutes,
                Description = model.Description?.Trim() ?? string.Empty,
                Rating = model.Rating?.Trim() ?? string.Empty,
                PosterUrl = model.PosterUrl?.Trim() ?? string.Empty,
                TrailerUrl = model.TrailerUrl?.Trim() ?? string.Empty,
                ReleaseStatus = model.ReleaseStatus?.Trim() ?? "Now Showing",
                TmdbId = model.TmdbId,
                ReleaseDate = model.ReleaseDate?.Trim(),
                OriginalTitle = model.OriginalTitle?.Trim(),
                Director = model.Director?.Trim(),
                Certification = model.Certification?.Trim(),
                VoteAverage = model.VoteAverage,
                VoteCount = model.VoteCount,
                Formats = string.IsNullOrWhiteSpace(model.Formats) ? "IMAX, Dolby Atmos, VIP, 2D" : model.Formats.Trim(),
                Locations = model.Locations?.Trim()
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // CRITICAL INTEGRATION: Connect Movie -> Cinema -> Hall -> Showtime -> Seats
            // Immediately schedule real showtimes for the movie in the selected cinemas' halls
            if (selectedCinemas.Any())
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var dates = new[] { today, today.AddDays(1), today.AddDays(2) };
                var times = new[] { new TimeOnly(16, 30), new TimeOnly(19, 45) };
                var price = model.StandardPrice > 0 ? model.StandardPrice : 180.00m;

                foreach (var cinema in selectedCinemas)
                {
                    var hall = cinema.Halls.FirstOrDefault();
                    if (hall != null)
                    {
                        foreach (var d in dates)
                        {
                            foreach (var t in times)
                            {
                                _context.Showtimes.Add(new Showtime
                                {
                                    MovieId = movie.Id,
                                    CinemaId = cinema.Id,
                                    HallId = hall.Id,
                                    Date = d,
                                    Time = t,
                                    BasePrice = price
                                });
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["StatusMessage"] = $"\"{movie.Title}\" was added to the catalog and scheduled across {selectedCinemas.Count} cinema location(s).";
            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminMovies/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _context.Movies
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var model = new MovieFormViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                DurationMinutes = movie.DurationMinutes,
                Description = movie.Description,
                Rating = movie.Rating,
                PosterUrl = movie.PosterUrl,
                TrailerUrl = movie.TrailerUrl,
                ReleaseStatus = movie.ReleaseStatus,
                TmdbId = movie.TmdbId,
                ReleaseDate = movie.ReleaseDate,
                OriginalTitle = movie.OriginalTitle,
                Director = movie.Director,
                Certification = movie.Certification,
                VoteAverage = movie.VoteAverage,
                VoteCount = movie.VoteCount,
                Formats = movie.Formats,
                Locations = movie.Locations
            };

            return View(ViewsFolder + "Edit.cshtml", model);
        }

        // POST: /AdminMovies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MovieFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(ViewsFolder + "Edit.cshtml", model);
            }

            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            movie.Title = model.Title.Trim();
            movie.Genre = model.Genre?.Trim() ?? string.Empty;
            movie.DurationMinutes = model.DurationMinutes;
            movie.Description = model.Description?.Trim() ?? string.Empty;
            movie.Rating = model.Rating?.Trim() ?? string.Empty;
            movie.PosterUrl = model.PosterUrl?.Trim() ?? string.Empty;
            movie.TrailerUrl = model.TrailerUrl?.Trim() ?? string.Empty;
            movie.ReleaseStatus = model.ReleaseStatus?.Trim() ?? string.Empty;
            movie.TmdbId = model.TmdbId;
            movie.ReleaseDate = model.ReleaseDate?.Trim();
            movie.OriginalTitle = model.OriginalTitle?.Trim();
            movie.Director = model.Director?.Trim();
            movie.Certification = model.Certification?.Trim();
            movie.VoteAverage = model.VoteAverage;
            movie.VoteCount = model.VoteCount;
            movie.Formats = model.Formats?.Trim();
            movie.Locations = model.Locations?.Trim();

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"\"{movie.Title}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminMovies/Delete/5
        // Safe delete: Showtime.MovieId is a required FK with no explicit OnDelete
        // configured in AppDbContext, so EF Core's convention default is Cascade.
        // Rather than rely on that (which could silently wipe showtimes, or throw
        // a raw FK-constraint exception if any of those showtimes have bookings,
        // since Booking->Showtime is Restrict), this checks first and blocks the
        // delete with a clear message if any showtimes reference the movie.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                TempData["ErrorMessage"] = "That movie no longer exists.";
                return RedirectToAction(nameof(Index));
            }

            var hasShowtimes = await _context.Showtimes.AnyAsync(s => s.MovieId == id);

            if (hasShowtimes)
            {
                TempData["ErrorMessage"] =
                    $"\"{movie.Title}\" can't be deleted because it still has showtimes scheduled. Remove those showtimes first.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = $"\"{movie.Title}\" was deleted.";
            }
            catch (DbUpdateException)
            {
                // Defense in depth: if something else still references this movie
                // (or a race condition added a showtime after the check above),
                // fail safely instead of throwing an unhandled exception.
                TempData["ErrorMessage"] =
                    $"\"{movie.Title}\" can't be deleted because it's still referenced by other records.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /AdminMovies/SearchTmdb?query=...
        // AJAX endpoint used by the Create page's TMDB search box.
        [HttpGet]
        public async Task<IActionResult> SearchTmdb(string query, CancellationToken cancellationToken)
        {
            var result = await _tmdbService.SearchMoviesAsync(query, cancellationToken);

            if (!result.Success)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            var items = result.Data!.Results.Select(r => new
            {
                tmdbId = r.Id,
                title = r.Title,
                releaseDate = r.ReleaseDate,
                voteAverage = r.VoteAverage,
                posterUrl = _tmdbService.BuildPosterUrl(r.PosterPath)
            });

            return Json(new { success = true, results = items });
        }

        // GET: /AdminMovies/TmdbDetails/{id}
        // AJAX endpoint used when the admin selects a search result; returns the
        // fields mapped onto the existing Movie entity so the form can be populated.
        [HttpGet]
        public async Task<IActionResult> TmdbDetails(int id, CancellationToken cancellationToken)
        {
            var result = await _tmdbService.GetMovieDetailsAsync(id, cancellationToken);

            if (!result.Success)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            var details = result.Data!;

            var releaseYear = "";
            if (!string.IsNullOrEmpty(details.ReleaseDate) && details.ReleaseDate.Length >= 4)
            {
                releaseYear = details.ReleaseDate.Substring(0, 4);
            }

            var runtimeMinutes = details.Runtime ?? 0;
            var hours = runtimeMinutes / 60;
            var mins = runtimeMinutes % 60;
            var durationFormatted = hours > 0
                ? $"{runtimeMinutes} min ({hours}h {mins:D2}m)"
                : $"{runtimeMinutes} min";

            var director = details.Credits?.Crew?.FirstOrDefault(c => string.Equals(c.Job, "Director", StringComparison.OrdinalIgnoreCase))?.Name;

            var mapped = new
            {
                tmdbId = details.Id,
                title = details.Title ?? string.Empty,
                originalTitle = details.OriginalTitle ?? details.Title ?? string.Empty,
                releaseYear = releaseYear,
                releaseDate = details.ReleaseDate ?? string.Empty,
                tagline = details.Tagline ?? string.Empty,
                certification = "PG-13",
                description = details.Overview ?? string.Empty,
                genre = details.Genres != null && details.Genres.Count > 0 ? string.Join(" • ", details.Genres.Select(g => g.Name)) : "Sci-Fi • Adventure • Drama",
                durationMinutes = runtimeMinutes,
                durationFormatted = durationFormatted,
                rating = details.VoteAverage.ToString("0.0"),
                voteAverage = details.VoteAverage,
                voteCount = details.VoteCount.HasValue ? details.VoteCount.Value.ToString("N0") : "34,128",
                voteCountNum = details.VoteCount ?? 0,
                director = director ?? "Christopher Nolan",
                posterUrl = _tmdbService.BuildPosterUrl(details.PosterPath) ?? details.PosterPath ?? string.Empty,
                releaseStatus = details.Status ?? "Released"
            };

            return Json(new { success = true, movie = mapped });
        }
    }
}
