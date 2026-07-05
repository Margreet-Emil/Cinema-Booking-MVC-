using System.ComponentModel.DataAnnotations;

namespace CinemaBookingSystem.Models
{
    public class Cinema
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Cinema name is required.")]
        [StringLength(100, ErrorMessage = "Cinema name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        public string Address { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

       
        public ICollection<Hall> Halls { get; set; } = new List<Hall>();
    }
}