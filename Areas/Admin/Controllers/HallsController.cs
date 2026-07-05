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
    public class HallsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HallsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? cinemaId)
        {
            var query = _context.Halls.Include(h => h.Cinema).AsQueryable();

            if (cinemaId.HasValue)
                query = query.Where(h => h.CinemaId == cinemaId.Value);

            var halls = await query.OrderBy(h => h.Cinema!.Name).ThenBy(h => h.Name).ToListAsync();

            ViewBag.CinemaId = cinemaId;
            return View(halls);
        }

        private async Task PopulateCinemasDropdown(int? selectedId = null)
        {
            var cinemas = await _context.Cinemas.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Cinemas = new SelectList(cinemas, "Id", "Name", selectedId);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateCinemasDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Hall hall)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCinemasDropdown(hall.CinemaId);
                return View(hall);
            }

            _context.Halls.Add(hall);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hall created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall == null)
                return NotFound();

            await PopulateCinemasDropdown(hall.CinemaId);
            return View(hall);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Hall hall)
        {
            if (id != hall.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateCinemasDropdown(hall.CinemaId);
                return View(hall);
            }

            _context.Halls.Update(hall);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hall updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var hall = await _context.Halls.Include(h => h.Cinema).FirstOrDefaultAsync(h => h.Id == id);
            if (hall == null)
                return NotFound();

            return View(hall);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall == null)
                return NotFound();

            var hasShowtimes = await _context.Showtimes.AnyAsync(s => s.HallId == id);
            if (hasShowtimes)
            {
                TempData["ErrorMessage"] = "Cannot delete this hall because it has showtimes scheduled.";
                return RedirectToAction(nameof(Index));
            }

            _context.Halls.Remove(hall);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hall deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}