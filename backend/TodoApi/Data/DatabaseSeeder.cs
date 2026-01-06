using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public static class DatabaseSeeder
{
    public static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        // Check if categories already exist
        if (await context.Categories.AnyAsync())
            return;

        var categories = new[]
        {
            new Category
            {
                Name = "Work",
                Color = "#3B82F6", // Blue
                Description = "Work-related tasks",
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Name = "Personal",
                Color = "#10B981", // Green
                Description = "Personal tasks",
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Name = "Shopping",
                Color = "#F59E0B", // Amber
                Description = "Shopping tasks",
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Name = "Health",
                Color = "#EF4444", // Red
                Description = "Health and fitness tasks",
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}

