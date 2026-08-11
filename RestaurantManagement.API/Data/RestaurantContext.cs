using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.API.Models;

namespace RestaurantManagement.API.Data
{
    public class RestaurantContext : IdentityDbContext<ApplicationUser>
    {
        public RestaurantContext(DbContextOptions<RestaurantContext> options)
            : base(options)
        {
        }

        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<Restaurant> Restaurants => Set<Restaurant>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<CartItem> CartItems => Set<CartItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(u => u.Restaurant)
                      .WithMany(r => r.Admins)
                      .HasForeignKey(u => u.RestaurantId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(u => u.Orders)
                      .WithOne(o => o.User)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.CartItems)
                      .WithOne(c => c.User)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Restaurant>(entity =>
            {
                entity.HasMany(r => r.MenuItems)
                      .WithOne(m => m.Restaurant)
                      .HasForeignKey(m => m.RestaurantId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(r => r.Orders)
                      .WithOne(o => o.Restaurant)
                      .HasForeignKey(o => o.RestaurantId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<MenuItem>(entity =>
            {
                entity.HasMany(m => m.OrderItems)
                      .WithOne(oi => oi.MenuItem)
                      .HasForeignKey(oi => oi.MenuItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(m => m.CartItems)
                      .WithOne(c => c.MenuItem)
                      .HasForeignKey(c => c.MenuItemId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Order>(entity =>
            {
                entity.HasMany(o => o.OrderItems)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CartItem>(entity =>
            {
                entity.HasIndex(c => new { c.UserId, c.MenuItemId }).IsUnique();
            });

            var roles = new[]
            {
                new IdentityRole("SuperAdmin") { NormalizedName = "SUPERADMIN" },
                new IdentityRole("RestaurantAdmin") { NormalizedName = "RESTAURANTADMIN" },
                new IdentityRole("User") { NormalizedName = "USER" }
            };

            var restaurants = new[]
            {
                new Restaurant { Id = 1, Name = "Riverside Bistro", Address = "123 Riverfront Drive", IsActive = true },
                new Restaurant { Id = 2, Name = "City Garden Cafe", Address = "456 Market Street", IsActive = true }
            };

            var superAdminId = "5a7c40f5-5d62-4339-8dfa-1e2b0e5e0f1a";
            var restaurantAdmin1Id = "7811c297-d44f-4c9d-8a8b-1f0f2e4a39b3";
            var restaurantAdmin2Id = "d4e96531-b998-4f5d-9c24-a0e1a0f2b4b9";
            var normalUserId = "a1b2c3d4-e5f6-4711-8c9d-0a1b2c3d4e5f";

            var superAdmin = CreateSeedUser(superAdminId, "superadmin@restaurant.local", "SuperAdmin", null, "SuperAdmin!23");
            var restaurantAdmin1 = CreateSeedUser(restaurantAdmin1Id, "restaurant1-admin@restaurant.local", "RestaurantAdmin1", 1, "RestaurantAdmin!23");
            var restaurantAdmin2 = CreateSeedUser(restaurantAdmin2Id, "restaurant2-admin@restaurant.local", "RestaurantAdmin2", 2, "RestaurantAdmin!23");
            var normalUser = CreateSeedUser(normalUserId, "user@restaurant.local", "User1", null, "User!23");

            builder.Entity<IdentityRole>().HasData(roles);
            builder.Entity<Restaurant>().HasData(restaurants);
            var menuItems = new[]
            {
                new MenuItem { Id = 1, Name = "Riverside Salad", Description = "Fresh greens with vinaigrette", Price = 8.99m, IsAvailable = true, RestaurantId = 1 },
                new MenuItem { Id = 2, Name = "River Grilled Salmon", Description = "Served with seasonal vegetables", Price = 18.50m, IsAvailable = true, RestaurantId = 1 },
                new MenuItem { Id = 3, Name = "City Garden Sandwich", Description = "House special sandwich", Price = 9.75m, IsAvailable = true, RestaurantId = 2 },
                new MenuItem { Id = 4, Name = "Garden Latte", Description = "Local roast coffee", Price = 3.50m, IsAvailable = true, RestaurantId = 2 }
            };
            builder.Entity<MenuItem>().HasData(menuItems);
            builder.Entity<ApplicationUser>().HasData(superAdmin, restaurantAdmin1, restaurantAdmin2, normalUser);
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { RoleId = roles[0].Id, UserId = superAdminId },
                new IdentityUserRole<string> { RoleId = roles[1].Id, UserId = restaurantAdmin1Id },
                new IdentityUserRole<string> { RoleId = roles[1].Id, UserId = restaurantAdmin2Id },
                new IdentityUserRole<string> { RoleId = roles[2].Id, UserId = normalUserId }
            );
        }

        private static ApplicationUser CreateSeedUser(string id, string email, string userName, int? restaurantId, string password)
        {
            var user = new ApplicationUser
            {
                Id = id,
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                RestaurantId = restaurantId
            };

            user.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(user, password);
            return user;
        }
    }
}
