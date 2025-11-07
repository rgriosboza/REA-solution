// REA.API/Services/UserSeederService.cs
using Microsoft.EntityFrameworkCore;
using REA.Models.Data;
using REA.Models.Entities;

namespace REA.API.Services
{
    public interface IUserSeederService
    {
        Task SeedInitialUsersAsync();
    }

    public class UserSeederService : IUserSeederService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserSeederService> _logger;

        public UserSeederService(ApplicationDbContext context, ILogger<UserSeederService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedInitialUsersAsync()
        {
            try
            {
                // Check if users already exist
                if (await _context.Users.AnyAsync())
                {
                    _logger.LogInformation("Users already exist. Skipping seed.");
                    return;
                }

                _logger.LogInformation("Seeding initial users...");

                var users = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        Email = "admin@reasystem.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                        Role = "Director",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Username = "viceprincipal",
                        Email = "vice@reasystem.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                        Role = "VicePrincipal",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Username = "teacher",
                        Email = "teacher@reasystem.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                        Role = "Teacher",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await _context.Users.AddRangeAsync(users);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully seeded {Count} users", users.Count);
                _logger.LogWarning("⚠️  Default password is 'Admin123!' - CHANGE THIS IN PRODUCTION!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding users");
                throw;
            }
        }
    }
}

// Add this to Program.cs after var app = builder.Build();
/*
// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var seeder = services.GetRequiredService<IUserSeederService>();
        await seeder.SeedInitialUsersAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB");
    }
}
*/