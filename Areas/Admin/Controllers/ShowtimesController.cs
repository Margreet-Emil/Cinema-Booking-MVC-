using CinemaBookingSystem.Constants;
using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class ShowtimesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShowtimesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? movieId)
        {
            var query = _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h!.Cinema)
                .AsQueryable();

            if (movieId.HasValue)
                query = query.Where(s => s.MovieId == movieId.Value);

            var showtimes = await query.OrderBy(s => s.StartTime).ToListAsync();

            ViewBag.MovieId = movieId;
            return View(showtimes);
        }

        private async Task PopulateDropdowns(int? movieId = null, int? hallId = null)
        {
            var movies = await _context.Movies.OrderBy(m => m.Title).ToListAsync();
            var halls = await _context.Halls.Include(h => h.Cinema).OrderBy(h => h.Cinema!.Name).ThenBy(h => h.Name).ToListAsync();

            ViewBag.Movies = new SelectList(movies, "Id", "Title", movieId);
            ViewBag.Halls = new SelectList(
                halls.Select(h => new { h.Id, DisplayName = $"{h.Cinema!.Name} - {h.Name}" }),
                "Id", "DisplayName", hallId);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Showtime showtime)
        {
            if (showtime.StartTime <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(showtime.StartTime), "Showtime must be scheduled in the future.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(showtime.MovieId, showtime.HallId);
                return View(showtime);
            }

            _context.Showtimes.Add(showtime);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Showtime created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime == null)
                return NotFound();

            await PopulateDropdowns(showtime.MovieId, showtime.HallId);
            return View(showtime);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Showtime showtime)
        {
            if (id != showtime.Id)
                return NotFound();

            if (showtime.StartTime <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(showtime.StartTime), "Showtime must be scheduled in the future.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(showtime.MovieId, showtime.HallId);
                return View(showtime);
            }

            _context.Showtimes.Update(showtime);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Showtime updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h!.Cinema)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (showtime == null)
                return NotFound();

            return View(showtime);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime == null)
                return NotFound();

            var hasBookings = await _context.Bookings.AnyAsync(b => b.ShowtimeId == id && b.Status == BookingStatus.Confirmed);
            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this showtime because it has active bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Showtimes.Remove(showtime);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Showtime deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}