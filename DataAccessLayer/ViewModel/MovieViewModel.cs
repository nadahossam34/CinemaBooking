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
        public string ReleaseStatus { get; set; }
        public string? Description { get; set; }

        // Derived from the real Movie columns (Formats, ReleaseDate, VoteAverage,
        // Certification) added via the TMDB import flow - never hardcoded. Left
        // null/0 when a movie hasn't been enriched with TMDB data yet; the views'
        // own display-level fallbacks (e.g. "IMAX 3D" placeholder badge) handle
        // presentation - this is real DB-derived data, not fake data.
        public string? Format { get; set; }
        public int Year { get; set; }
        public double Score { get; set; }
        public string? AgeRating { get; set; }
    }
}
