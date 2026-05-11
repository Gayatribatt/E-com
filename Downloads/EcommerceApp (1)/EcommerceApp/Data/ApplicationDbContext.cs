using EcommerceApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            builder.Entity<Product>()
                .Property(p => p.OriginalPrice)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.Total)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.SubTotal)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.Tax)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.ShippingCost)
                .HasPrecision(18, 2);

            builder.Entity<OrderItem>()
                .Property(o => o.UnitPrice)
                .HasPrecision(18, 2);
        }
    }

    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Electronics", Description = "Latest gadgets and electronics", ImageUrl = "/images/electronics.jpg" },
                    new() { Name = "Clothing", Description = "Fashion for all occasions", ImageUrl = "/images/clothing.jpg" },
                    new() { Name = "Books", Description = "Books for every reader", ImageUrl = "/images/books.jpg" },
                    new() { Name = "Home & Garden", Description = "Everything for your home", ImageUrl = "/images/home.jpg" },
                    new() { Name = "Sports", Description = "Sports and outdoor equipment", ImageUrl = "/images/sports.jpg" }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();

                var products = new List<Product>
                {
                    new() { Name = "Wireless Headphones", Description = "Premium noise-cancelling wireless headphones with 30-hour battery life.", Price = 299.99m, OriginalPrice = 399.99m, StockQuantity = 50, CategoryId = 1, IsFeatured = true, ImageUrl = "/images/headphones.jpg" },
                    new() { Name = "Laptop Pro 15\"", Description = "High-performance laptop with Intel i7, 16GB RAM, 512GB SSD.", Price = 1299.99m, StockQuantity = 20, CategoryId = 1, IsFeatured = true, ImageUrl = "/images/laptop.jpg" },
                    new() { Name = "Smartphone X", Description = "Latest flagship smartphone with 5G support and 108MP camera.", Price = 899.99m, OriginalPrice = 999.99m, StockQuantity = 75, CategoryId = 1, IsFeatured = true, ImageUrl = "/images/phone.jpg" },
                    new() { Name = "Smartwatch Pro", Description = "Advanced smartwatch with health monitoring and GPS.", Price = 249.99m, StockQuantity = 40, CategoryId = 1, ImageUrl = "/images/watch.jpg" },
                    new() { Name = "Men's Casual Shirt", Description = "Premium cotton casual shirt, available in multiple colors.", Price = 49.99m, StockQuantity = 100, CategoryId = 2, ImageUrl = "/images/shirt.jpg" },
                    new() { Name = "Women's Dress", Description = "Elegant evening dress for special occasions.", Price = 89.99m, OriginalPrice = 129.99m, StockQuantity = 60, CategoryId = 2, IsFeatured = true, ImageUrl = "/images/dress.jpg" },
                    new() { Name = "Running Shoes", Description = "Lightweight and durable running shoes for all terrains.", Price = 119.99m, StockQuantity = 80, CategoryId = 2, ImageUrl = "/images/shoes.jpg" },
                    new() { Name = "The Art of Programming", Description = "Comprehensive guide to modern software development.", Price = 39.99m, StockQuantity = 200, CategoryId = 3, ImageUrl = "/images/book1.jpg" },
                    new() { Name = "Business Strategy", Description = "Learn proven strategies for business success.", Price = 29.99m, StockQuantity = 150, CategoryId = 3, ImageUrl = "/images/book2.jpg" },
                    new() { Name = "Coffee Maker Deluxe", Description = "12-cup programmable coffee maker with built-in grinder.", Price = 79.99m, StockQuantity = 35, CategoryId = 4, IsFeatured = true, ImageUrl = "/images/coffee.jpg" },
                    new() { Name = "Garden Tool Set", Description = "Complete 10-piece garden tool set with storage bag.", Price = 59.99m, StockQuantity = 45, CategoryId = 4, ImageUrl = "/images/tools.jpg" },
                    new() { Name = "Yoga Mat Premium", Description = "Non-slip premium yoga mat with carrying strap.", Price = 34.99m, StockQuantity = 90, CategoryId = 5, ImageUrl = "/images/yoga.jpg" },
                    new() { Name = "Camping Tent 4-Person", Description = "Waterproof 4-person camping tent with easy setup.", Price = 149.99m, OriginalPrice = 199.99m, StockQuantity = 25, CategoryId = 5, IsFeatured = true, ImageUrl = "/images/tent.jpg" },
                };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
