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
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<UserCredit> UserCredits { get; set; }
        public DbSet<CreditHistory> CreditHistories { get; set; }

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
            
            // UserPreference configuration
            builder.Entity<UserPreference>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.UserId)
                      .IsRequired();
                      
                entity.Property(e => e.Theme)
                      .HasMaxLength(20);
                      
                entity.Property(e => e.Language)
                      .HasMaxLength(10);
                      
                entity.Property(e => e.PreferredPaymentMethod)
                      .HasMaxLength(50);
                      
                entity.Property(e => e.DietaryRestrictions)
                      .HasMaxLength(500);
                      
                entity.Property(e => e.FavoriteCategories)
                      .HasMaxLength(300);
                      
                entity.Property(e => e.AllergyInformation)
                      .HasMaxLength(500);
                      
                entity.Property(e => e.DefaultDeliveryAddress)
                      .HasMaxLength(300);
                      
                entity.Property(e => e.DeliveryInstructions)
                      .HasMaxLength(500);
                
                entity.HasIndex(e => e.UserId).IsUnique();
            });
            
            // Feedback configuration
            builder.Entity<Feedback>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.UserId)
                      .IsRequired();
                      
                entity.Property(e => e.Subject)
                      .IsRequired()
                      .HasMaxLength(100);
                      
                entity.Property(e => e.Message)
                      .IsRequired()
                      .HasMaxLength(2000);
                      
                entity.Property(e => e.AdminResponse)
                      .HasMaxLength(2000);
                
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Status);
            });
            
            // MenuItem configuration
            builder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);
                      
                entity.Property(e => e.Description)
                      .HasMaxLength(500);
                      
                entity.Property(e => e.Price)
                      .IsRequired()
                      .HasColumnType("decimal(10,2)");
                      
                entity.Property(e => e.ImageUrl)
                      .HasMaxLength(200);
                      
                entity.Property(e => e.Category)
                      .HasMaxLength(50);
                      
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.IsAvailable);
            });
            
            // Order configuration
            builder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.OrderNumber)
                      .IsRequired()
                      .HasMaxLength(50);
                      
                entity.Property(e => e.UserId)
                      .IsRequired();
                      
                entity.Property(e => e.TotalAmount)
                      .HasColumnType("decimal(10,2)");
                      
                entity.Property(e => e.PaymentMethod)
                      .IsRequired()
                      .HasMaxLength(50);
                      
                entity.Property(e => e.TransactionId)
                      .IsRequired()
                      .HasMaxLength(100);
                      
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.OrderDate);
                entity.HasIndex(e => e.Status);
            });
            
            // OrderItem configuration
            builder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.MenuItemName)
                      .IsRequired()
                      .HasMaxLength(100);
                      
                entity.Property(e => e.UnitPrice)
                      .HasColumnType("decimal(10,2)");
                      
                entity.HasOne(e => e.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasIndex(e => e.OrderId);
            });
            
            // UserCredit configuration
            builder.Entity<UserCredit>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.UserId)
                      .IsRequired();
                      
                entity.Property(e => e.CurrentCredits)
                      .HasColumnType("decimal(10,2)");
                      
                entity.Property(e => e.TotalEarned)
                      .HasColumnType("decimal(10,2)");
                      
                entity.Property(e => e.TotalSpent)
                      .HasColumnType("decimal(10,2)");
                
                entity.HasIndex(e => e.UserId).IsUnique();
            });
            
            // CreditHistory configuration
            builder.Entity<CreditHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.UserId)
                      .IsRequired();
                      
                entity.Property(e => e.Amount)
                      .HasColumnType("decimal(10,2)");
                      
                entity.Property(e => e.Type)
                      .IsRequired()
                      .HasMaxLength(20);
                      
                entity.Property(e => e.Description)
                      .IsRequired()
                      .HasMaxLength(200);
                      
                entity.Property(e => e.BalanceAfter)
                      .HasColumnType("decimal(10,2)");
                
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Type);
            });
        }
    }
}
