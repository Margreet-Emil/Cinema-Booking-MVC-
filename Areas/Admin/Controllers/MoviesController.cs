using CinemaBookingSystem.Constants;
using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using CinemaBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Roles.Admin)]
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MoviesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task PopulateCategoriesDropdown(int? selectedId = null)
        {
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedId);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Include(m => m.Category)
                .OrderBy(m => m.Title)
                .ToListAsync();

            return View(movies);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesDropdown();
            return View(new MovieFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropdown(vm.CategoryId);
                return View(vm);
            }

            var movie = new Movie
            {
                Title = vm.Title,
                Description = vm.Description,
                DurationMinutes = vm.DurationMinutes,
                CategoryId = vm.CategoryId
            };

            if (vm.PosterFile != null && vm.PosterFile.Length > 0)
            {
                movie.PosterPath = await SavePosterFile(vm.PosterFile);
            }

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Movie created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();

            var vm = new MovieFormViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                DurationMinutes = movie.DurationMinutes,
                CategoryId = movie.CategoryId,
                ExistingPosterPath = movie.PosterPath
            };

            await PopulateCategoriesDropdown(movie.CategoryId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MovieFormViewModel vm)
        {
            if (id != vm.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesDropdown(vm.CategoryId);
                return View(vm);
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();

            movie.Title = vm.Title;
            movie.Description = vm.Description;
            movie.DurationMinutes = vm.DurationMinutes;
            movie.CategoryId = vm.CategoryId;

            if (vm.PosterFile != null && vm.PosterFile.Length > 0)
            {
                DeletePosterFile(movie.PosterPath);
                movie.PosterPath = await SavePosterFile(vm.PosterFile);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Movie updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies.Include(m => m.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
                return NotFound();

            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();

            var hasShowtimes = await _context.Showtimes.AnyAsync(s => s.MovieId == id);
            if (hasShowtimes)
            {
                TempData["ErrorMessage"] = "Cannot delete this movie because it has showtimes scheduled.";
                return RedirectToAction(nameof(Index));
            }

            DeletePosterFile(movie.PosterPath);

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Movie deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Helper methods for poster upload ----------

        private async Task<string> SavePosterFile(IFormFile posterFile)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "posters");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(posterFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await posterFile.CopyToAsync(stream);
            }

            return $"/uploads/posters/{uniqueFileName}";
        }

        private void DeletePosterFile(string? posterPath)
        {
            if (string.IsNullOrEmpty(posterPath))
                return;

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, posterPath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}