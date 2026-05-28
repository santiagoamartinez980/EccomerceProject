using APIEccomerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace APIEccomerce.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
           
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2); 

            modelBuilder.Entity<Category>()
                .HasKey(c => c.CategoryId);

            modelBuilder.Entity<Product>()
                .HasKey(p => p.ProductId);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Address>()
                .HasOne(d => d.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Address>()
                .Property(d => d.Latitude)
                .HasPrecision(9, 6);

            modelBuilder.Entity<Address>()
                .Property(d => d.Longitude)
                .HasPrecision(9, 6);
        }
    }
}
