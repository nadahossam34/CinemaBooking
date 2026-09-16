using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.ViewModels
{
    /// <summary>
    /// Mirrors the existing Movie entity (DataAccessLayer/Models/Movie.cs) field-for-field.
    /// Used for both Add Movie and Edit Movie (Id is unused/0 on Create).
    /// Populated either manually or via the TMDB search/select workflow on the Create page.
    /// </summary>
    public class MovieFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Genre cannot exceed 200 characters.")]
        public string? Genre { get; set; }

        [Range(0, 1000, ErrorMessage = "Duration must be between 0 and 1000 minutes.")]
        [Display(Name = "Duration (minutes)")]
        public int DurationMinutes { get; set; }

        [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters.")]
        public string? Description { get; set; }

        [StringLength(20, ErrorMessage = "Rating cannot exceed 20 characters.")]
        public string? Rating { get; set; }

        [StringLength(500, ErrorMessage = "Poster URL cannot exceed 500 characters.")]
        [Display(Name = "Poster URL")]
        public string? PosterUrl { get; set; }

        [StringLength(500, ErrorMessage = "Trailer URL cannot exceed 500 characters.")]
        [Display(Name = "Trailer URL")]
        public string? TrailerUrl { get; set; }

        [StringLength(50, ErrorMessage = "Release status cannot exceed 50 characters.")]
        [Display(Name = "Release Status")]
        public string? ReleaseStatus { get; set; }

        [Display(Name = "TMDB ID")]
        public int TmdbId { get; set; }
    }
}
