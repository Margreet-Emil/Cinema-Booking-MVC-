using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using CinemaBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (showtime == null)
                return NotFound();

            if (showtime.StartTime <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "This showtime has already started and can no longer be booked.";
                return RedirectToAction("Details", "Movies", new { id = showtime.MovieId });
            }

            int seatsTaken = showtime.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed)
                .Sum(b => b.NumberOfSeats);
            int availableSeats = showtime.Hall!.SeatCapacity - seatsTaken;

            var vm = new BookingCreateViewModel
            {
                ShowtimeId = showtime.Id,
                MovieTitle = showtime.Movie!.Title,
                CinemaName = showtime.Hall.Cinema!.Name,
                HallName = showtime.Hall.Name,
                StartTime = showtime.StartTime,
                TicketPrice = showtime.TicketPrice,
                AvailableSeats = availableSeats,
                NumberOfSeats = 1
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingCreateViewModel vm)
        {
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Cinema)
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == vm.ShowtimeId);

            if (showtime == null)
                return NotFound();

            vm.MovieTitle = showtime.Movie!.Title;
            vm.CinemaName = showtime.Hall!.Cinema!.Name;
            vm.HallName = showtime.Hall.Name;
            vm.StartTime = showtime.StartTime;
            vm.TicketPrice = showtime.TicketPrice;

            int seatsTaken = showtime.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed)
                .Sum(b => b.NumberOfSeats);
            int availableSeats = showtime.Hall.SeatCapacity - seatsTaken;
            vm.AvailableSeats = availableSeats;

            if (showtime.StartTime <= DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "This showtime has already started and can no longer be booked.");
            }

            if (vm.NumberOfSeats > availableSeats)
            {
                ModelState.AddModelError(nameof(vm.NumberOfSeats), $"Only {availableSeats} seat(s) left for this showtime.");
            }

            if (!ModelState.IsValid)
                return View(vm);

            var userId = _userManager.GetUserId(User);

            var booking = new Booking
            {
                ShowtimeId = showtime.Id,
                UserId = userId!,
                NumberOfSeats = vm.NumberOfSeats,
                TotalPrice = showtime.TicketPrice * vm.NumberOfSeats,
                BookingDate = DateTime.Now,
                Status = BookingStatus.Confirmed
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your booking was confirmed successfully.";
            return RedirectToAction(nameof(MyBookings));
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userId = _userManager.GetUserId(User);

            var bookings = await _context.Bookings
                .Include(b => b.Showtime)
                    .ThenInclude(s => s!.Movie)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s!.Hall)
                        .ThenInclude(h => h!.Cinema)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Showtime!.StartTime)
                .ToListAsync();

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);

            var booking = await _context.Bookings
                .Include(b => b.Showtime)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            if (booking.UserId != userId)
                return Forbid();

            if (booking.Status == BookingStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "This booking is already cancelled.";
                return RedirectToAction(nameof(MyBookings));
            }

            if (booking.Showtime!.StartTime <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "You cannot cancel a booking after the showtime has started.";
                return RedirectToAction(nameof(MyBookings));
            }

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your booking was cancelled successfully.";
            return RedirectToAction(nameof(MyBookings));
        }
    }
}