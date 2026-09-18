using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Controllers
{
    // Reuses the existing authorization mechanism: User.IsAdmin (DataAccessLayer/Models/User.cs)
    // is mapped onto a "Admin" role claim at sign-in time in AccountController.SignInUserAsync
    // (Phase 2). No second user system, no duplicate entity, no username-based checks.
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // GET: /Admin
        public IActionResult Index()
        {
            return View();
        }
    }
}
