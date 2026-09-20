using DataAccessLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuisnessLogicLayer.Service
{
    public interface ICinemaService
    {
        Task<List<CinemaViewModel>> GetAllCinemasAsync();

        Task<CinemaViewModel?> GetCinemaByIdAsync(int id);

        Task<NearestCinemaViewModel?> GetNearestCinemaAsync(
            double userLatitude,
            double userLongitude);
    }
}
