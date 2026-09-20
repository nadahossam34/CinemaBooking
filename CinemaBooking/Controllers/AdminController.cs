using CinemaBooking.Data;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard, /Admin, /Admin/Index
        [HttpGet]
        [Route("Admin/Dashboard")]
        [Route("Admin")]
        [Route("Admin/Index")]
        public async Task<IActionResult> Dashboard()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var stats = new AdminDashboardViewModel
            {
                TotalMovies = await _context.Movies.CountAsync(),
                TotalCinemas = await _context.Cinemas.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalBookings = await _context.Bookings.CountAsync(),
                TotalRevenue = await _context.BookingSeats.SumAsync(bs => (decimal?)bs.PricePaid) ?? 0m,
                TicketsSold = await _context.BookingSeats.CountAsync(),
                UpcomingShowtimesCount = await _context.Showtimes.CountAsync(s => s.Date >= today),
                RecentBookings = await _context.Bookings
                    .AsNoTracking()
                    .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                    .Include(b => b.Showtime).ThenInclude(s => s.Cinema)
                    .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(6)
                    .ToListAsync(),
                RecentMovies = await _context.Movies
                    .AsNoTracking()
                    .OrderByDescending(m => m.Id)
                    .Take(5)
                    .ToListAsync()
            };

            return View("Dashboard", stats);
        }

        // GET: /Admin/Showtimes
        [HttpGet]
        [Route("Admin/Showtimes")]
        public async Task<IActionResult> Showtimes()
        {
            var showtimes = await _context.Showtimes
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.Cinema)
                .Include(s => s.Hall)
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.BookingSeats)
                .OrderByDescending(s => s.Date)
                .ThenByDescending(s => s.Time)
                .ToListAsync();

            ViewBag.Movies = await _context.Movies.AsNoTracking().OrderBy(m => m.Title).ToListAsync();
            ViewBag.Cinemas = await _context.Cinemas.AsNoTracking().Include(c => c.Halls).OrderBy(c => c.Name).ToListAsync();

            return View(showtimes);
        }

        // POST: /Admin/CreateShowtime
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/CreateShowtime")]
        public async Task<IActionResult> CreateShowtime(int movieId, int cinemaId, int hallId, string date, string time, decimal basePrice)
        {
            if (movieId <= 0 || cinemaId <= 0 || hallId <= 0 || string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(time))
            {
                TempData["ErrorMessage"] = "Please provide all required showtime fields.";
                return RedirectToAction(nameof(Showtimes));
            }

            if (!DateOnly.TryParse(date, out var parsedDate))
            {
                TempData["ErrorMessage"] = "Invalid date format.";
                return RedirectToAction(nameof(Showtimes));
            }

            if (!TimeOnly.TryParse(time, out var parsedTime))
            {
                TempData["ErrorMessage"] = "Invalid time format.";
                return RedirectToAction(nameof(Showtimes));
            }

            var showtime = new global::Showtime
            {
                MovieId = movieId,
                CinemaId = cinemaId,
                HallId = hallId,
                Date = parsedDate,
                Time = parsedTime,
                BasePrice = basePrice > 0 ? basePrice : 180.00m
            };

            _context.Showtimes.Add(showtime);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "New showtime successfully scheduled.";
            return RedirectToAction(nameof(Showtimes));
        }

        // POST: /Admin/DeleteShowtime
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/DeleteShowtime")]
        public async Task<IActionResult> DeleteShowtime(int id)
        {
            var showtime = await _context.Showtimes.Include(s => s.Bookings).FirstOrDefaultAsync(s => s.Id == id);
            if (showtime == null)
            {
                TempData["ErrorMessage"] = "Showtime not found.";
                return RedirectToAction(nameof(Showtimes));
            }

            if (showtime.Bookings.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete showtime because customers have already booked tickets for it.";
                return RedirectToAction(nameof(Showtimes));
            }

            _context.Showtimes.Remove(showtime);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Showtime removed.";
            return RedirectToAction(nameof(Showtimes));
        }

        // GET: /Admin/Bookings
        [HttpGet]
        [Route("Admin/Bookings")]
        public async Task<IActionResult> Bookings()
        {
            var bookings = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.User)
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.Showtime).ThenInclude(s => s.Cinema)
                .Include(b => b.Showtime).ThenInclude(s => s.Hall)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .Include(b => b.Payment)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        // GET: /Admin/Concessions
        [HttpGet]
        [Route("Admin/Concessions")]
        public IActionResult Concessions()
        {
            return View();
        }

        // GET: /Admin/Reports
        [HttpGet]
        [Route("Admin/Reports")]
        public async Task<IActionResult> Reports()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var totalRev = await _context.BookingSeats.SumAsync(bs => (decimal?)bs.PricePaid) ?? 0m;
            var totalTickets = await _context.BookingSeats.CountAsync();
            var totalBookings = await _context.Bookings.CountAsync();

            var cinemaStats = await _context.Cinemas
                .AsNoTracking()
                .Select(c => new
                {
                    CinemaName = c.Name,
                    City = c.City,
                    ShowtimesCount = c.Showtimes.Count(s => s.Date >= today),
                    BookingsCount = c.Showtimes.SelectMany(s => s.Bookings).Count(),
                    Revenue = c.Showtimes.SelectMany(s => s.Bookings).SelectMany(b => b.BookingSeats).Sum(bs => (decimal?)bs.PricePaid) ?? 0m
                })
                .ToListAsync();

            ViewBag.TotalRevenue = totalRev;
            ViewBag.TotalTickets = totalTickets;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.CinemaStats = cinemaStats;

            return View();
        }

        // GET: /Admin/Settings
        [HttpGet]
        [Route("Admin/Settings")]
        public async Task<IActionResult> Settings()
        {
            ViewBag.TotalMovies = await _context.Movies.CountAsync();
            ViewBag.TotalCinemas = await _context.Cinemas.CountAsync();
            ViewBag.TotalHalls = await _context.Halls.CountAsync();
            ViewBag.TotalSeats = await _context.Seats.CountAsync();
            return View();
        }
    }
}
