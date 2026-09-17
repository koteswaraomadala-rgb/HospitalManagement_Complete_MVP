using HospitalManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (!await db.Users.AnyAsync())
        {
            db.Users.Add(new User
            {
                Username = "admin",
                Password = "admin123",
                FullName = "Administrator",
                Role = "Administrator"
            });
        }

        if (!await db.AppSettings.AnyAsync())
        {
            db.AppSettings.Add(new AppSetting());
        }

        await db.SaveChangesAsync();
    }
}
