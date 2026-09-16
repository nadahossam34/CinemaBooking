using System.Net.Http.Headers;
using CinemaBooking.Data;
using CinemaBooking.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using BuisnessLogicLayer.Service;

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
            builder.Services.AddScoped<IShowtimeService, ShowtimeService>(); // <--- ضفنا السطر هنا

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}