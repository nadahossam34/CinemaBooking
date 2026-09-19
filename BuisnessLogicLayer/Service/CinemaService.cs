using CinemaBooking.Data;
using DataAccessLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BuisnessLogicLayer.Service
{
    public class CinemaService : ICinemaService
    {
        private readonly AppDbContext _context;

        public CinemaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CinemaViewModel>> GetAllCinemasAsync()
        {
            try
            {
                var cinemas = await _context.Cinemas
                    .AsNoTracking()
                    .Select(c => new CinemaViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        City = c.City,
                        Address = c.Address,
                        Latitude = c.Latitude,
                        Longitude = c.Longitude
                    })
                    .ToListAsync();

                if (cinemas != null && cinemas.Any())
                    return cinemas;
            }
            catch (Exception)
            {
                // Graceful fallback if database is offline or not yet migrated
            }

            return GetDefaultCinemas();
        }

        public async Task<CinemaViewModel?> GetCinemaByIdAsync(int id)
        {
            try
            {
                var cinema = await _context.Cinemas
                    .AsNoTracking()
                    .Where(c => c.Id == id)
                    .Select(c => new CinemaViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        City = c.City,
                        Address = c.Address,
                        Latitude = c.Latitude,
                        Longitude = c.Longitude
                    })
                    .FirstOrDefaultAsync();

                if (cinema != null)
                    return cinema;
            }
            catch (Exception)
            {
                // Fallback
            }

            return GetDefaultCinemas().FirstOrDefault(c => c.Id == id);
        }

        public async Task<NearestCinemaViewModel?> GetNearestCinemaAsync(
            double userLatitude,
            double userLongitude)
        {
            List<CinemaViewModel> cinemas;
            try
            {
                cinemas = await _context.Cinemas
                    .AsNoTracking()
                    .Select(c => new CinemaViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        City = c.City,
                        Address = c.Address,
                        Latitude = c.Latitude,
                        Longitude = c.Longitude
                    })
                    .ToListAsync();

                if (cinemas == null || !cinemas.Any())
                    cinemas = GetDefaultCinemas();
            }
            catch (Exception)
            {
                cinemas = GetDefaultCinemas();
            }

            if (!cinemas.Any())
                return null;

            var nearest = cinemas
                .Select(c => new
                {
                    Cinema = c,
                    Distance = CalculateDistance(
                        userLatitude,
                        userLongitude,
                        (double)c.Latitude,
                        (double)c.Longitude)
                })
                .OrderBy(x => x.Distance)
                .First();

            return new NearestCinemaViewModel
            {
                Id = nearest.Cinema.Id,
                Name = nearest.Cinema.Name,
                City = nearest.Cinema.City,
                Address = nearest.Cinema.Address,
                Latitude = nearest.Cinema.Latitude,
                Longitude = nearest.Cinema.Longitude,
                DistanceKm = Math.Round(nearest.Distance, 1),
                UserLatitude = userLatitude,
                UserLongitude = userLongitude
            };
        }

        private static List<CinemaViewModel> GetDefaultCinemas()
        {
            return new List<CinemaViewModel>
            {
                new CinemaViewModel
                {
                    Id = 1,
                    Name = "StarLight Mall of Egypt",
                    City = "Giza",
                    Address = "Gate 4, Level 2, Mall of Egypt, Wahat Road, 6th of October City, Giza",
                    Latitude = 29.9723m,
                    Longitude = 31.0189m
                },
                new CinemaViewModel
                {
                    Id = 2,
                    Name = "StarLight New Cairo",
                    City = "New Cairo",
                    Address = "The Promenade, Cairo Festival City Mall, Ring Road, New Cairo",
                    Latitude = 30.0298m,
                    Longitude = 31.4087m
                },
                new CinemaViewModel
                {
                    Id = 3,
                    Name = "StarLight Alexandria",
                    City = "Alexandria",
                    Address = "San Stefano Grand Plaza, 3rd Floor, El-Geish Road, Alexandria",
                    Latitude = 31.2443m,
                    Longitude = 29.9686m
                }
            };
        }

        private static double CalculateDistance(
            double lat1,
            double lon1,
            double lat2,
            double lon2)
        {
            const double earthRadiusKm = 6371;

            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) *
                Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
