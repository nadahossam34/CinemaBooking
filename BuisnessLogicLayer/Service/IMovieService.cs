using DataAccessLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuisnessLogicLayer.Service
{
    public interface IMovieService
    {
        Task<List<MovieViewModel>> GetAllMoviesAsync();
        Task<MovieDetailsViewModel?> GetMovieByIdAsync(int id);
    }
}
