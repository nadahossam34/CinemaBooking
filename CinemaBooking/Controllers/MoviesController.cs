using BuisnessLogicLayer.Service;
using CinemaBooking.Data;
using CinemaBooking.Services;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers
{
    public class MoviesController : Controller
    {
        private const string ViewsFolder = "~/Views/Admin/Movies/";

        private readonly AppDbContext _context;
        private readonly ITmdbService _tmdbService;
        private readonly IMovieService _movieService;

        public MoviesController(AppDbContext context, ITmdbService tmdbService, IMovieService movieService)
        {
            _context = context;
            _tmdbService = tmdbService;
            _movieService = movieService;
        }

        // ==========================================
        // Customer-facing actions (Stitch Design UI)
        // ==========================================

        // GET: /Movies
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? genre)
        {
            var movies = await _movieService.GetAllMoviesAsync();
            ViewData["InitialSearch"] = search ?? "";
            ViewData["InitialGenre"] = genre ?? "";
            return View(movies);
        }

        // GET: /Movies/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // ==========================================
        // Admin management actions
        // ==========================================

        // GET: /Movies/Manage
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var movies = await _context.Movies
                .AsNoTracking()
                .OrderBy(m => m.Title)
                .ToListAsync();

            return View(ViewsFolder + "Index.cshtml", movies);
        }

        // GET: /Movies/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(ViewsFolder + "Create.cshtml", new MovieFormViewModel());
        }

        // POST: /Movies/Create
        [Authorize(Roles = "Admin")]
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
            return RedirectToAction(nameof(Manage));
        }

        // GET: /Movies/Edit/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
            return RedirectToAction(nameof(Manage));
        }

        // POST: /Movies/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                TempData["ErrorMessage"] = "That movie no longer exists.";
                return RedirectToAction(nameof(Manage));
            }

            var hasShowtimes = await _context.Showtimes.AnyAsync(s => s.MovieId == id);

            if (hasShowtimes)
            {
                TempData["ErrorMessage"] =
                    $"\"{movie.Title}\" can't be deleted because it still has showtimes scheduled. Remove those showtimes first.";
                return RedirectToAction(nameof(Manage));
            }

            try
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = $"\"{movie.Title}\" was deleted.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    $"\"{movie.Title}\" can't be deleted because it's still referenced by other records.";
            }

            return RedirectToAction(nameof(Manage));
        }

        // GET: /Movies/SearchTmdb?query=...
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
