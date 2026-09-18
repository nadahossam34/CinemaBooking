using CinemaBooking.Data;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using BuisnessLogicLayer.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers
namespace PresentationLayer.Controllers
{
    // Views live under Views/Admin/Cinemas/ (explicit paths below), same approach
    // as MoviesController in Phase 5.
    [Authorize(Roles = "Admin")]
    public class CinemasController : Controller
    {
        private const string ViewsFolder = "~/Views/Admin/Cinemas/";
        private readonly ICinemaService _cinemaService;

        private readonly AppDbContext _context;

        public CinemasController(AppDbContext context)
        public CinemasController(ICinemaService cinemaService)
        {
            _context = context;
            _cinemaService = cinemaService;
        }

        // GET: /Cinemas
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cinemas = await _context.Cinemas
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
            var cinemas = await _cinemaService.GetAllCinemasAsync();

            return View(ViewsFolder + "Index.cshtml", cinemas);
            return View(cinemas);
        }

        // GET: /Cinemas/Create
        [HttpGet]
        public IActionResult Create()
        public async Task<IActionResult> Details(int id)
        {
            return View(ViewsFolder + "Create.cshtml", new CinemaFormViewModel());
        }
            var cinema = await _cinemaService.GetCinemaByIdAsync(id);

        // POST: /Cinemas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CinemaFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(ViewsFolder + "Create.cshtml", model);
            }

            var cinema = new Cinema
            {
                Name = model.Name.Trim(),
                City = model.City.Trim(),
                Address = model.Address.Trim(),
                Latitude = model.Latitude,
                Longitude = model.Longitude
            };

            _context.Cinemas.Add(cinema);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"\"{cinema.Name}\" was added.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cinemas/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cinema = await _context.Cinemas
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cinema == null)
            {
                return NotFound();
            }

            var model = new CinemaFormViewModel
            {
                Id = cinema.Id,
                Name = cinema.Name,
                City = cinema.City,
                Address = cinema.Address,
                Latitude = cinema.Latitude,
                Longitude = cinema.Longitude
            };

            return View(ViewsFolder + "Edit.cshtml", model);
        }

        // POST: /Cinemas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CinemaFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            return View(cinema);
        }

            if (!ModelState.IsValid)
        [HttpGet]
        public async Task<IActionResult> Nearest(
            double latitude,
            double longitude)
        {
                return View(ViewsFolder + "Edit.cshtml", model);
            }

            var cinema = await _context.Cinemas.FindAsync(id);
            var cinema = await _cinemaService
                .GetNearestCinemaAsync(latitude, longitude);

            if (cinema == null)
            {
                return NotFound();
            }

            cinema.Name = model.Name.Trim();
            cinema.City = model.City.Trim();
            cinema.Address = model.Address.Trim();
            cinema.Latitude = model.Latitude;
            cinema.Longitude = model.Longitude;

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"\"{cinema.Name}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cinemas/Delete/5
        //
        // Cinema sits above two separate relationship chains:
        //   Cinema -> Hall (required FK, no OnDelete configured -> EF default Cascade)
        //       Hall -> Seat (required FK, unconfigured -> Cascade)
        //           Seat -> BookingSeat (required FK, unconfigured -> Cascade)
        //       Hall -> Showtime (required FK, unconfigured -> Cascade)
        //   Cinema -> Showtime (required FK, explicitly Restrict in AppDbContext)
        //
        // The direct Cinema->Showtime FK is Restrict, but Halls sit on a fully
        // Cascade path that reaches Seats, BookingSeats, and even Showtimes
        // (via Hall.Id, independently of the Restrict on Cinema.Id). Relying on
        // the schema alone risks silently deleting seat/booking history through
        // that indirect path. So, without touching the schema, this blocks the
        // delete up front if the cinema still has ANY Halls or ANY Showtimes
        // referencing it, rather than letting cascade/restrict sort it out.
        //
        // Note: this project has no "Ticket" or "Screen" entity - the closest
        // real equivalents are Booking (via Showtime) and Hall, both covered here.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);

            if (cinema == null)
            {
                TempData["ErrorMessage"] = "That cinema no longer exists.";
                return RedirectToAction(nameof(Index));
            }

            var hasHalls = await _context.Halls.AnyAsync(h => h.CinemaId == id);
            var hasShowtimes = await _context.Showtimes.AnyAsync(s => s.CinemaId == id);

            if (hasHalls || hasShowtimes)
            {
                var blockers = new List<string>();
                if (hasHalls) blockers.Add("halls");
                if (hasShowtimes) blockers.Add("showtimes");

                TempData["ErrorMessage"] =
                    $"\"{cinema.Name}\" can't be deleted because it still has {string.Join(" and ", blockers)} associated with it. Remove those first.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Cinemas.Remove(cinema);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = $"\"{cinema.Name}\" was deleted.";
            }
            catch (DbUpdateException)
            {
                // Defense in depth: catches any late FK violation (e.g. a race
                // condition adding a hall/showtime after the check above)
                // instead of letting it surface as an unhandled exception.
                TempData["ErrorMessage"] =
                    $"\"{cinema.Name}\" can't be deleted because it's still referenced by other records.";
            }

            return RedirectToAction(nameof(Index));
            return View(cinema);
        }
    }
}
