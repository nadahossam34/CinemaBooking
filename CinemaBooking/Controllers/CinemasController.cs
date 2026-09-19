using BuisnessLogicLayer.Service;
using CinemaBooking.Data;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers
{
    public class CinemasController : Controller
    {
        private const string ViewsFolder = "~/Views/Admin/Cinemas/";
        private readonly ICinemaService _cinemaService;
        private readonly AppDbContext _context;

        public CinemasController(ICinemaService cinemaService, AppDbContext context)
        {
            _cinemaService = cinemaService;
            _context = context;
        }

        // ==========================================
        // Customer-facing actions (Stitch Design UI)
        // ==========================================

        // GET: /Cinemas
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cinemas = await _cinemaService.GetAllCinemasAsync();
            return View(cinemas);
        }

        // GET: /Cinemas/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var cinema = await _cinemaService.GetCinemaByIdAsync(id);
            if (cinema == null)
            {
                return NotFound();
            }

            return View(cinema);
        }

        // GET: /Cinemas/Nearest?latitude=...&longitude=...
        [HttpGet]
        public async Task<IActionResult> Nearest(double latitude, double longitude)
        {
            var cinema = await _cinemaService.GetNearestCinemaAsync(latitude, longitude);
            if (cinema == null)
            {
                return NotFound();
            }

            return View(cinema);
        }

        // GET: /Cinemas/GetCinemasJson — returns all cinema locations as JSON for the interactive map
        [HttpGet]
        public async Task<IActionResult> GetCinemasJson()
        {
            var cinemas = await _cinemaService.GetAllCinemasAsync();
            var result = cinemas.Select(c => new
            {
                c.Id,
                c.Name,
                c.City,
                c.Address,
                lat = (double)c.Latitude,
                lng = (double)c.Longitude
            });
            return Json(result);
        }

        // GET: /Cinemas/NearestJson?latitude=...&longitude=... — returns nearest cinema as JSON for AJAX
        [HttpGet]
        public async Task<IActionResult> NearestJson(double latitude, double longitude)
        {
            var cinema = await _cinemaService.GetNearestCinemaAsync(latitude, longitude);
            if (cinema == null)
            {
                return Json(null);
            }
            return Json(new
            {
                cinema.Id,
                cinema.Name,
                cinema.City,
                cinema.Address,
                lat = (double)cinema.Latitude,
                lng = (double)cinema.Longitude,
                cinema.DistanceKm
            });
        }

        // ==========================================
        // Admin management actions
        // ==========================================

        // GET: /Cinemas/Manage
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var cinemas = await _context.Cinemas
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(ViewsFolder + "Index.cshtml", cinemas);
        }

        // GET: /Cinemas/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(ViewsFolder + "Create.cshtml", new CinemaFormViewModel());
        }

        // POST: /Cinemas/Create
        [Authorize(Roles = "Admin")]
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
            return RedirectToAction(nameof(Manage));
        }

        // GET: /Cinemas/Edit/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
            return RedirectToAction(nameof(Manage));
        }

        // POST: /Cinemas/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);

            if (cinema == null)
            {
                TempData["ErrorMessage"] = "That cinema no longer exists.";
                return RedirectToAction(nameof(Manage));
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
                return RedirectToAction(nameof(Manage));
            }

            try
            {
                _context.Cinemas.Remove(cinema);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = $"\"{cinema.Name}\" was deleted.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    $"\"{cinema.Name}\" can't be deleted because it's still referenced by other records.";
            }

            return RedirectToAction(nameof(Manage));
        }
    }
}
