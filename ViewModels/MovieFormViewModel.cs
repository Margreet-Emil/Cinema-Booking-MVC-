using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CinemaBookingSystem.ViewModels
{
    public class MovieFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the movie title")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a description")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the duration")]
        [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes")]
        [Display(Name = "Duration (minutes)")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public IEnumerable<SelectListItem>? Categories { get; set; }

        [Display(Name = "Poster")]
        public IFormFile? PosterFile { get; set; }

      
        public string? ExistingPosterPath { get; set; }

        [Required(ErrorMessage = "Please select a release date")]
        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [Display(Name = "Is Showing")]
        public bool IsShowing { get; set; } = true;
    }
}