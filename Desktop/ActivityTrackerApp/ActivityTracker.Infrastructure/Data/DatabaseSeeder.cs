using ActivityTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace ActivityTracker.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext, IPasswordHasher<AppUser> passwordHasher, CancellationToken cancellationToken = default)
    {
        if (dbContext.Users.Any(x => x.Username == "admin"))
        {
            return;
        }

        var admin = new AppUser
        {
            Username = "admin",
            Role = "Admin"
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin@123");
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
