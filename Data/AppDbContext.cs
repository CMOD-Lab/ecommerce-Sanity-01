using EcommerceWebApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace EcommerceWebApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default schema to public
            modelBuilder.HasDefaultSchema("public");

            // Configure PostgreSQL extensions
            modelBuilder.HasPostgresExtension("uuid-ossp");

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar(500)")
                    .IsRequired();
                entity.Property(e => e.Price)
                    .HasColumnName("price")
                    .HasColumnType("numeric(18,2)")
                    .IsRequired();
                entity.Property(e => e.Rating)
                    .HasColumnName("rating")
                    .HasColumnType("numeric(3,2)");
                entity.Property(e => e.Brand)
                    .HasColumnName("brand")
                    .HasColumnType("varchar(255)")
                    .IsRequired();
                entity.Property(e => e.Category)
                    .HasColumnName("category")
                    .HasColumnType("varchar(255)");
                entity.Property(e => e.Thumbnail)
                    .HasColumnName("thumbnail")
                    .HasColumnType("text")
                    .HasConversion(
                        v => v == null ? null : v.ToString(),
                        v => v == null ? null! : new Uri(v));
                entity.Property(e => e.Quantity)
                    .HasColumnName("quantity")
                    .HasColumnType("integer")
                    .IsRequired();
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("varchar(36)")
                    .IsRequired();
                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .HasColumnType("varchar(255)")
                    .IsRequired();
                entity.Property(e => e.Role)
                    .HasColumnName("role")
                    .HasColumnType("varchar(50)")
                    .IsRequired();
                entity.Property(e => e.IsTwoFactorAuthActivated)
                    .HasColumnName("is_two_factor_auth_activated")
                    .HasColumnType("boolean")
                    .HasDefaultValue(false);
                entity.Property(e => e.PasswordSalt)
                    .HasColumnName("password_salt")
                    .HasColumnType("bytea")
                    .IsRequired();
                entity.Property(e => e.PasswordHash)
                    .HasColumnName("password_hash")
                    .HasColumnType("bytea")
                    .IsRequired();
                entity.Property(e => e.SecretCode)
                    .HasColumnName("secret_code")
                    .HasColumnType("varchar(255)");

                // Configure one-to-one relationship with RefreshToken
                entity.HasOne(e => e.RefreshToken)
                    .WithOne(r => r.User)
                    .HasForeignKey<RefreshToken>(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure RefreshToken entity
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("varchar(36)")
                    .IsRequired();
                entity.Property(e => e.Token)
                    .HasColumnName("token")
                    .HasColumnType("text");
                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone");
                entity.Property(e => e.Expires)
                    .HasColumnName("expires")
                    .HasColumnType("timestamp with time zone");
            });

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("varchar(36)")
                    .IsRequired();
                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("varchar(36)")
                    .IsRequired();
                entity.Property(e => e.ProductList)
                    .HasColumnName("product_list")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<Dictionary<int, int>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<int, int>());
                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("timestamp with time zone")
                    .IsRequired();
                entity.Property(e => e.Updated)
                    .HasColumnName("updated")
                    .HasColumnType("timestamp with time zone");
                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("integer")
                    .IsRequired();
            });
        }
    }
}
