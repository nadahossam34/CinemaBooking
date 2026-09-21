using System.Net.Http.Headers;
using CinemaBooking.Data;
using CinemaBooking.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using BuisnessLogicLayer.Service;
using BuisnessLogicLayer.Services;

namespace CinemaBooking
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Existing AppDbContext (DataAccessLayer) was not previously wired into DI.
            // Registered here using the existing "DefaultConnection" connection string.
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Cookie-based authentication. No ASP.NET Core Identity package exists in this
            // project, so this is the smallest addition that fits the existing architecture
            // and works directly against the existing custom User entity.
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                });

            // TMDB integration (Phase 4). The API read access token comes from
            // configuration only - user secrets locally (dotnet user-secrets set
            // "Tmdb:ApiReadAccessToken" "...") or an environment variable /
            // secret store in production. It is never hard-coded here and never
            // sent to the browser - only this typed HttpClient attaches it,
            // server-side, as a Bearer header.
            builder.Services.Configure<TmdbOptions>(builder.Configuration.GetSection(TmdbOptions.SectionName));

            builder.Services.AddHttpClient<ITmdbService, TmdbService>((serviceProvider, client) =>
            {
                var tmdbOptions = serviceProvider.GetRequiredService<IOptions<TmdbOptions>>().Value;

                client.BaseAddress = new Uri(tmdbOptions.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrWhiteSpace(tmdbOptions.ApiReadAccessToken))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", tmdbOptions.ApiReadAccessToken);
                }
            });
            builder.Services.AddScoped<IMovieService, MovieService>();
            builder.Services.AddScoped<ICinemaService, CinemaService>();

            builder.Services.AddScoped<BookingService>();
            builder.Services.AddScoped<PaymentService>();
            builder.Services.AddScoped<QRCodeService>();

            builder.Services.AddScoped<IShowtimeService, ShowtimeService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "adminDashboard",
                pattern: "Admin/Dashboard",
                defaults: new { controller = "Admin", action = "Dashboard" });

            app.MapControllerRoute(
                name: "adminShowtimes",
                pattern: "Admin/Showtimes",
                defaults: new { controller = "Admin", action = "Showtimes" });

            app.MapControllerRoute(
                name: "adminBookings",
                pattern: "Admin/Bookings",
                defaults: new { controller = "Admin", action = "Bookings" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();

                    // 1. Ensure Admin User
                    if (!db.Users.Any(u => u.Email == "admin@starlight.com"))
                    {
                        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<global::User>();
                        var admin = new global::User
                        {
                            Name = "Admin Tarek Mansour",
                            Email = "admin@starlight.com",
                            IsAdmin = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
                        db.Users.Add(admin);
                        db.SaveChanges();
                    }

                    // 2. Ensure SeatTypes
                    if (!db.SeatTypes.Any())
                    {
                        db.SeatTypes.AddRange(
                            new global::SeatType { Name = "Standard", PriceMultiplier = 1.0m },
                            new global::SeatType { Name = "VIP", PriceMultiplier = 1.5m },
                            new global::SeatType { Name = "IMAX", PriceMultiplier = 1.25m }
                        );
                        db.SaveChanges();
                    }

                    var standardSeatType = db.SeatTypes.First(st => st.Name == "Standard");
                    var vipSeatType = db.SeatTypes.FirstOrDefault(st => st.Name == "VIP") ?? standardSeatType;

                    // 3. Ensure Seats for all Halls
                    var halls = db.Halls.Include(h => h.Seats).ToList();
                    var newSeats = new List<global::Seat>();
                    foreach (var hall in halls)
                    {
                        if (!hall.Seats.Any())
                        {
                            var rows = new[] { "A", "B", "C", "D", "E" };
                            foreach (var row in rows)
                            {
                                for (int num = 1; num <= 8; num++)
                                {
                                    newSeats.Add(new global::Seat
                                    {
                                        HallId = hall.Id,
                                        RowLabel = row,
                                        SeatNumber = num,
                                        SeatTypeId = (row == "E") ? vipSeatType.Id : standardSeatType.Id
                                    });
                                }
                            }
                        }
                    }
                    if (newSeats.Any())
                    {
                        db.Seats.AddRange(newSeats);
                        db.SaveChanges();
                    }

                    // 4. Ensure initial showtimes exist for ALL catalog movies so customer booking is immediately live
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var allMovies = db.Movies.ToList();
                    var cinemaHalls = db.Halls.Include(h => h.Cinema).ToList();

                    if (allMovies.Any() && cinemaHalls.Any())
                    {
                        var initialShowtimes = new List<global::Showtime>();
                        var times = new[] { new TimeOnly(15, 0), new TimeOnly(18, 30), new TimeOnly(21, 15) };
                        var dates = new[] { today, today.AddDays(1), today.AddDays(2) };

                        foreach (var movie in allMovies)
                        {
                            if (!db.Showtimes.Any(s => s.MovieId == movie.Id && s.Date >= today))
                            {
                                foreach (var date in dates)
                                {
                                    foreach (var hall in cinemaHalls.Take(2))
                                    {
                                        foreach (var time in times.Take(2))
                                        {
                                            initialShowtimes.Add(new global::Showtime
                                            {
                                                MovieId = movie.Id,
                                                CinemaId = hall.CinemaId,
                                                HallId = hall.Id,
                                                Date = date,
                                                Time = time,
                                                BasePrice = 180.00m
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        if (initialShowtimes.Any())
                        {
                            db.Showtimes.AddRange(initialShowtimes);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning(ex, "Could not ensure database seeded on startup.");
                }
            }

            app.Run();
        }
    }
}