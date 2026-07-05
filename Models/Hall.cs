using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBookingSystem.Models
{
    public class Hall
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hall name is required.")]
        [StringLength(50, ErrorMessage = "Hall name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seat capacity is required.")]
        [Range(1, 1000, ErrorMessage = "Seat capacity must be between 1 and 1000.")]
        [Display(Name = "Seat Capacity")]
        public int SeatCapacity { get; set; }

        [Required(ErrorMessage = "Please select a cinema.")]
        [Display(Name = "Cinema")]
        public int CinemaId { get; set; }

        [ForeignKey(nameof(CinemaId))]
        public Cinema? Cinema { get; set; }

  
        public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}