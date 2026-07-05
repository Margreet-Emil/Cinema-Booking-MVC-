using System.ComponentModel.DataAnnotations;

namespace CinemaBookingSystem.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;

       
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}