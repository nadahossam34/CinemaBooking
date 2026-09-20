using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.ViewModel
{
    public class MovieDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; }
        public string Rating { get; set; }
        public string PosterUrl { get; set; }
        public string TrailerUrl { get; set; }
        public string ReleaseStatus { get; set; }
        public int TmdbId { get; set; }

        public List<CinemaViewModel> Cinemas { get; set; } = new();
    }
}
