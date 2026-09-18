using CinemaBooking.Data;
using CinemaBooking.Services;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers
{
    // Views live under Views/Admin/Movies/ (explicit paths below) rather than the
    // conventional Views/Movies/, per the Phase 5 requirement.
    [Authorize(Roles = "Admin")]
    public class MoviesController : Controller
    {
        private const string ViewsFolder = "~/Views/Admin/Movies/";

        private readonly AppDbContext _context;
        private readonly ITmdbService _tmdbService;

        public MoviesController(AppDbContext context, ITmdbService tmdbService)
        {
            _context = context;
            _tmdbService = tmdbService;
        }

        // GET: /Movies
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .AsNoTracking()
                .OrderBy(m => m.Title)
                .ToListAsync();

            return View(ViewsFolder + "Index.cshtml", movies);
        }

        // GET: /Movies/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(ViewsFolder + "Create.cshtml", new MovieFormViewModel());
        }

        // POST: /Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(ViewsFolder + "Create.cshtml", model);
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
                ReleaseStatus = model.ReleaseStatus?.Trim() ?? string.Empty,
                TmdbId = model.TmdbId
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"\"{movie.Title}\" was added to the catalog.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Movies/Edit/5
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
                TmdbId = movie.TmdbId
            };

            return View(ViewsFolder + "Edit.cshtml", model);
        }

        // POST: /Movies/Edit/5
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

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"\"{movie.Title}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Movies/Delete/5
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

        // GET: /Movies/SearchTmdb?query=...
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

        // GET: /Movies/TmdbDetails/{id}
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

            var mapped = new
            {
                tmdbId = details.Id,
                title = details.Title,
                description = details.Overview,
                genre = string.Join(", ", details.Genres.Select(g => g.Name)),
                durationMinutes = details.Runtime ?? 0,
                rating = details.VoteAverage.ToString("0.0"),
                posterUrl = _tmdbService.BuildPosterUrl(details.PosterPath),
                releaseStatus = details.Status
            };

            return Json(new { success = true, movie = mapped });
        }
    }
}
