using EcommerceWebApi.Data;
using EcommerceWebApi.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EcommerceWebApi.Data
{
    /// <summary>
    /// Seeds initial data from the legacy JSON flat file database into PostgreSQL.
    /// </summary>
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext context, ILogger logger)
        {
            try
            {
                // Ensure database is created and migrations applied
                await context.Database.MigrateAsync();

                // Seed products if none exist
                if (!await context.Products.AnyAsync())
                {
                    logger.LogInformation("Seeding products from legacy JSON data store...");
                    var products = GetSeedProducts();
                    await context.Products.AddRangeAsync(products);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Seeded {Count} products successfully.", products.Count);
                }

                // Seed default admin user if none exist
                if (!await context.Users.AnyAsync())
                {
                    logger.LogInformation("Seeding default admin user...");
                    var adminUser = CreateDefaultAdminUser();
                    await context.Users.AddAsync(adminUser);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Default admin user seeded successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database: {Message}", ex.Message);
                throw;
            }
        }

        private static User CreateDefaultAdminUser()
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            var passwordBytes = System.Text.Encoding.UTF8.GetBytes("Admin@123");
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = "admin",
                Role = "Admin",
                IsTwoFactorAuthActivated = false,
                PasswordSalt = hmac.Key,
                PasswordHash = hmac.ComputeHash(passwordBytes),
                SecretCode = null,
                RefreshToken = new RefreshToken
                {
                    Token = null,
                    Created = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddDays(7)
                }
            };
        }

        private static List<Product> GetSeedProducts()
        {
            return new List<Product>
            {
                new Product { Title = "iPhone 9", Price = 549.0f, Rating = 4.69f, Brand = "Apple", Category = "smartphones", Thumbnail = new Uri("https://i.dummyjson.com/data/products/1/thumbnail.jpg"), Quantity = 5 },
                new Product { Title = "iPhone X", Price = 899.0f, Rating = 4.44f, Brand = "Apple", Category = "smartphones", Thumbnail = new Uri("https://i.dummyjson.com/data/products/2/thumbnail.jpg"), Quantity = 1 },
                new Product { Title = "Samsung Universe 9", Price = 1249.0f, Rating = 4.09f, Brand = "Samsung", Category = "smartphones", Thumbnail = new Uri("https://i.dummyjson.com/data/products/3/thumbnail.jpg"), Quantity = 0 },
                new Product { Title = "OPPOF19", Price = 280.0f, Rating = 4.3f, Brand = "OPPO", Category = "smartphones", Thumbnail = new Uri("https://i.dummyjson.com/data/products/4/thumbnail.jpg"), Quantity = 8 },
                new Product { Title = "Huawei P30", Price = 499.0f, Rating = 4.09f, Brand = "Huawei", Category = "smartphones", Thumbnail = new Uri("https://i.dummyjson.com/data/products/5/thumbnail.jpg"), Quantity = 8 },
                new Product { Title = "MacBook Pro", Price = 1749.0f, Rating = 4.57f, Brand = "Apple", Category = "laptops", Thumbnail = new Uri("https://i.dummyjson.com/data/products/6/thumbnail.png"), Quantity = 6 },
                new Product { Title = "Samsung Galaxy Book", Price = 1499.0f, Rating = 4.25f, Brand = "Samsung", Category = "laptops", Thumbnail = new Uri("https://i.dummyjson.com/data/products/7/thumbnail.jpg"), Quantity = 8 },
                new Product { Title = "Microsoft Surface Laptop 4", Price = 1499.0f, Rating = 4.43f, Brand = "Microsoft Surface", Category = "laptops", Thumbnail = new Uri("https://i.dummyjson.com/data/products/8/thumbnail.jpg"), Quantity = 6 },
                new Product { Title = "Infinix INBOOK", Price = 1099.0f, Rating = 4.54f, Brand = "Infinix", Category = "laptops", Thumbnail = new Uri("https://i.dummyjson.com/data/products/9/thumbnail.jpg"), Quantity = 4 },
                new Product { Title = "HP Pavilion 15-DK1056WM", Price = 1099.0f, Rating = 4.43f, Brand = "HP Pavilion", Category = "laptops", Thumbnail = new Uri("https://i.dummyjson.com/data/products/10/thumbnail.jpeg"), Quantity = 0 }
            };
        }
    }
}
