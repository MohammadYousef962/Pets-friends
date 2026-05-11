using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pets_friends.Models;
using System.Linq;

namespace Pets_friends.Data
{
    public class AppDbContext : IdentityDbContext<UserAccount>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // --- 1. Profiles ---
        public DbSet<ClientProfile> ClientProfiles { get; set; }

        public DbSet<VetProfile> VetProfiles { get; set; }
        public DbSet<ShelterProfile> ShelterProfiles { get; set; }
        public DbSet<MerchantProfile> MerchantProfiles { get; set; }

        // --- 2. Core Business Entities ---
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<WorkingDay> WorkingDays { get; set; }
        public DbSet<VetReview> VetReviews { get; set; }

        // --- 3. Transactions & Interactions ---
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // CRITICAL: Always first

            // --- 1. Configure One-to-One Relationships ---
            builder.Entity<UserAccount>().HasOne(u => u.ClientProfile).WithOne(p => p.UserAccount).HasForeignKey<ClientProfile>(p => p.UserAccountId);
            builder.Entity<UserAccount>().HasOne(u => u.VetProfile).WithOne(p => p.UserAccount).HasForeignKey<VetProfile>(p => p.UserAccountId);
            builder.Entity<UserAccount>().HasOne(u => u.MerchantProfile).WithOne(p => p.UserAccount).HasForeignKey<MerchantProfile>(p => p.UserAccountId);
            builder.Entity<UserAccount>().HasOne(u => u.ShelterProfile).WithOne(p => p.UserAccount).HasForeignKey<ShelterProfile>(p => p.UserAccountId);

            // --- 2. Prevent Multiple Cascade Path Crashes ---
            builder.Entity<VetReview>().HasOne(r => r.Reviewer).WithMany().HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.NoAction);
            builder.Entity<CartItem>().HasOne(c => c.Product).WithMany().HasForeignKey(c => c.ProductId).OnDelete(DeleteBehavior.NoAction);
            builder.Entity<OrderItem>().HasOne(o => o.Product).WithMany().HasForeignKey(o => o.ProductId).OnDelete(DeleteBehavior.NoAction);

            // ---> NEW: Fixes the Error 1785 crash for Product Reviews <---
            builder.Entity<ProductReview>()
                .HasOne(pr => pr.ClientProfile)
                .WithMany()
                .HasForeignKey(pr => pr.ClientProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── THE ULTIMATE APPOINTMENT FIX ──
            // We must restrict ALL foreign keys on the Appointment table to stop the Diamond Loop.
            builder.Entity<Appointment>()
                .HasOne(a => a.ClientProfile)
                .WithMany()
                .HasForeignKey(a => a.ClientProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // ---> NEW: Fixes the Error 1785 crash for Orders <---
            builder.Entity<Order>()
                .HasOne(o => o.MerchantProfile)
                .WithMany()
                .HasForeignKey(o => o.MerchantProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Appointment>()
                .HasOne(a => a.Pet)
                .WithMany()
                .HasForeignKey(a => a.PetId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Appointment>()
                .HasOne(a => a.VetProfile)
                .WithMany(v => v.Appointments)
                .HasForeignKey(a => a.VetProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // --- 3. Global Decimal Precision for Money ---
            foreach (var property in builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }
    }
}