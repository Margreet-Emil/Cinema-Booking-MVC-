using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBookingSystem.Models
{
    public class Showtime
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "You must select a movie")]
        public int MovieId { get; set; }

        [ForeignKey(nameof(MovieId))]
        public Movie? Movie { get; set; }

        [Required(ErrorMessage = "You must select a hall")]
        public int HallId { get; set; }

        [ForeignKey(nameof(HallId))]
        public Hall? Hall { get; set; }

        [Required(ErrorMessage = "Showtime date/time is required")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required]
        [Range(0.01, 10000, ErrorMessage = "Price must be greater than zero")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TicketPrice { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        [NotMapped]
        public int SeatsBooked => Bookings?.Where(b => b.Status == BookingStatus.Confirmed)
                                            .Sum(b => b. NumberOfSeats) ?? 0;

        [NotMapped]
        public int AvailableSeats => (Hall?.SeatCapacity ?? 0) - SeatsBooked;

        [NotMapped]
        public bool HasStarted => DateTime.Now >= StartTime;
    }
}