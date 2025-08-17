using CampusCafeOrderingSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusCafeOrderingSystem.Data
{
    // Use non-generic IdentityDbContext (equivalent to IdentityDbContext<IdentityUser>)
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CateringApplication> CateringApplications { get; set; }
        public DbSet<CafeApp.Models.user_order_pay.Order> Orders { get; set; }
        public DbSet<CafeApp.Models.user_order_pay.OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // CateringApplication configuration
            builder.Entity<CateringApplication>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ContactName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.ContactPhone)
                      .IsRequired()
                      .HasMaxLength(40);

                entity.Property(e => e.Address)
                      .HasMaxLength(200);

                // Comma-separated dietary requirements
                entity.Property(e => e.DietaryCsv)
                      .HasMaxLength(300);

                entity.Property(e => e.Notes)
                      .HasMaxLength(1000);

                // Budget per person, set precision
                entity.Property(e => e.BudgetPerPerson)
                      .HasColumnType("decimal(10,2)");

                // Common indexes
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.EventDate);
            });
        }
    }
}
