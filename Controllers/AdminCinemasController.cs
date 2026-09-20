using CinemaBooking.Data;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers
{
    // Admin-only Cinema CRUD. Views live under Views/Admin/Cinemas/ (explicit paths below).
    //
    // Named "AdminCinemas" rather than "Cinemas" because the public, service-based
    // CinemasController (PresentationLayer.Controllers, from the movies/cinema branch)
    // already owns the "Cinemas" controller name. Two controllers with the same name
    // compile fine in different namespaces but make MVC throw AmbiguousMatchException
    // at request time, so the admin surface lives at /AdminCinemas instead.
    [Authorize(Roles = "Admin")]
    public class AdminCinemasController : Controller
    {
        private const string ViewsFolder = "~/Views/Admin/Cinemas/";

        private readonly AppDbContext _context;

        public AdminCinemasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /AdminCinemas
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cinemas = await _context.Cinemas
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(ViewsFolder + "Index.cshtml", cinemas);
        }

        // GET: /AdminCinemas/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(ViewsFolder + "Create.cshtml", new CinemaFormViewModel());
        }

        // POST: /AdminCinemas/Create
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

        // GET: /AdminCinemas/Edit/5
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

        // POST: /AdminCinemas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CinemaFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(ViewsFolder + "Edit.cshtml", model);
            }

            var cinema = await _context.Cinemas.FindAsync(id);

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

        // POST: /AdminCinemas/Delete/5
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
        }
    }
}
