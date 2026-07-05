using CinemaBookingSystem.Constants;
using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class CinemasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CinemasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cinemas = await _context.Cinemas
                .Include(c => c.Halls)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(cinemas);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cinema cinema)
        {
            if (!ModelState.IsValid)
                return View(cinema);

            _context.Cinemas.Add(cinema);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cinema created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
                return NotFound();

            return View(cinema);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cinema cinema)
        {
            if (id != cinema.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(cinema);

            _context.Cinemas.Update(cinema);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cinema updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
                return NotFound();

            return View(cinema);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
                return NotFound();

            var hasHalls = await _context.Halls.AnyAsync(h => h.CinemaId == id);
            if (hasHalls)
            {
                TempData["ErrorMessage"] = "Cannot delete this cinema because it has halls assigned to it.";
                return RedirectToAction(nameof(Index));
            }

            _context.Cinemas.Remove(cinema);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cinema deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}