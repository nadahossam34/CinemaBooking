using CinemaBooking.Data;
using DataAccessLayer.ViewModel;
using System;
using System.Collections.Generic;
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
            return await _context.Cinemas
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
        }

        public async Task<CinemaViewModel?> GetCinemaByIdAsync(int id)
        {
            return await _context.Cinemas
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
        }

        public async Task<NearestCinemaViewModel?> GetNearestCinemaAsync(
            double userLatitude,
            double userLongitude)
        {
            var cinemas = await _context.Cinemas
                .AsNoTracking()
                .ToListAsync();

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
