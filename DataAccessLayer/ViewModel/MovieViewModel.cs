using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.ViewModel
{
    public class MovieViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public int DurationMinutes { get; set; }
        public string Rating { get; set; }
        public string PosterUrl { get; set; }
        public string ReleaseStatus { get; set; } = "Now Showing";
        public string? Description { get; set; }
        public string? Format { get; set; }
        public int Year { get; set; } = 2025;
        public double Score { get; set; } = 8.8;
        public string? AgeRating { get; set; }
    }
}
