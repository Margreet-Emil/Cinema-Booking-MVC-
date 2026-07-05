using System.ComponentModel.DataAnnotations;

namespace CinemaBookingSystem.ViewModels
{
    public class BookingCreateViewModel
    {
        [Required]
        public int ShowtimeId { get; set; }

       
        public string MovieTitle { get; set; } = string.Empty;
        public string CinemaName { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public decimal TicketPrice { get; set; }
        public int AvailableSeats { get; set; }

        [Required(ErrorMessage = "Please enter the number of seats")]
        [Range(1, 10, ErrorMessage = "You can book between 1 and 10 seats")]
        [Display(Name = "Number of Seats")]
        public int NumberOfSeats { get; set; } = 1;

 
        public decimal TotalPrice => TicketPrice * NumberOfSeats;
    }
}