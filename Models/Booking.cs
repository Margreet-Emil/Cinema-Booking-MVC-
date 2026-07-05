using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Please select a showtime.")]
        [Display(Name = "Showtime")]
        public int ShowtimeId { get; set; }

        [ForeignKey(nameof(ShowtimeId))]
        public Showtime? Showtime { get; set; }

        [Required(ErrorMessage = "Number of seats is required.")]
        [Range(1, 20, ErrorMessage = "You can book between 1 and 20 seats.")]
        [Display(Name = "Number of Seats")]
        public int NumberOfSeats { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total Price")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    }
}