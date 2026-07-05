using CinemaBookingSystem.Constants;
using CinemaBookingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Data
{
    public static class SeedData
    {
        private const string AdminEmail = "admin@cinema.com";
        private const string AdminPassword = "Admin@12345";

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ---------- Roles ----------
            foreach (var role in new[] { Roles.Admin, Roles.Customer })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ---------- Default Admin Account ----------
            var adminUser = await userManager.FindByEmailAsync(AdminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, AdminPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create the default admin account: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }

            // ---------- Sample Data (only if tables are empty) ----------
            if (!await context.Categories.AnyAsync())
            {
                var action = new Category { Name = "Action" };
                var drama = new Category { Name = "Drama" };
                var comedy = new Category { Name = "Comedy" };

                context.Categories.AddRange(action, drama, comedy);
                await context.SaveChangesAsync();

                var cinema1 = new Cinema { Name = "Star Cinema", Address = "Ismailia, Egypt" };
                var cinema2 = new Cinema { Name = "Golden Screen", Address = "Cairo, Egypt" };

                context.Cinemas.AddRange(cinema1, cinema2);
                await context.SaveChangesAsync();

                var hall1 = new Hall { Name = "Hall 1", SeatCapacity = 50, CinemaId = cinema1.Id };
                var hall2 = new Hall { Name = "Hall 2", SeatCapacity = 80, CinemaId = cinema1.Id };
                var hall3 = new Hall { Name = "VIP Hall", SeatCapacity = 30, CinemaId = cinema2.Id };

                context.Halls.AddRange(hall1, hall2, hall3);
                await context.SaveChangesAsync();

                var movie1 = new Movie
                {
                    Title = "The Last Horizon",
                    Description = "A thrilling journey across time and space to save humanity.",
                    DurationMinutes = 128,
                    CategoryId = action.Id
                };

                var movie2 = new Movie
                {
                    Title = "Silent Echoes",
                    Description = "A touching story about family, loss, and hope.",
                    DurationMinutes = 105,
                    CategoryId = drama.Id
                };

                var movie3 = new Movie
                {
                    Title = "Laugh Out Loud",
                    Description = "A hilarious comedy about a mismatched pair of roommates.",
                    DurationMinutes = 95,
                    CategoryId = comedy.Id
                };

                context.Movies.AddRange(movie1, movie2, movie3);
                await context.SaveChangesAsync();

                var showtimes = new List<Showtime>
                {
                    new Showtime { MovieId = movie1.Id, HallId = hall1.Id, StartTime = DateTime.Now.AddDays(1).AddHours(3), TicketPrice = 100 },
                    new Showtime { MovieId = movie1.Id, HallId = hall2.Id, StartTime = DateTime.Now.AddDays(2).AddHours(5), TicketPrice = 100 },
                    new Showtime { MovieId = movie2.Id, HallId = hall2.Id, StartTime = DateTime.Now.AddDays(1).AddHours(6), TicketPrice = 80 },
                    new Showtime { MovieId = movie3.Id, HallId = hall3.Id, StartTime = DateTime.Now.AddDays(3).AddHours(4), TicketPrice = 120 }
                };

                context.Showtimes.AddRange(showtimes);
                await context.SaveChangesAsync();
            }
        }
    }
}