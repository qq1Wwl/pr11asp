using Microsoft.EntityFrameworkCore;
using pr9ASP.Models;

namespace pr9ASP.Data
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext(DbContextOptions<ShopDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(e =>
            {
                e.ToTable("Roles");
                e.HasKey(x => x.Role_Id);
                e.Property(x => x.RoleName).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(x => x.User_Id);
                e.Property(x => x.Login).IsRequired().HasMaxLength(50);
                e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(256);

                // Role (1) --- (N) User
                e.HasOne(x => x.Role).WithMany(r => r.Users)
                    .HasForeignKey(x => x.Role_Id).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Category>(e =>
            {
                e.ToTable("Categories");
                e.HasKey(x => x.Category_Id);
                e.Property(x => x.CategoryName).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Product>(e =>
            {
                e.ToTable("Products");
                e.HasKey(x => x.Product_Id);
                e.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
                e.Property(x => x.Price).HasColumnType("decimal(18,2)").IsRequired();

                // Category (1) --- (N) Product
                e.HasOne(x => x.Category).WithMany(c => c.Products)
                    .HasForeignKey(x => x.Category_Id).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Cart>(e =>
            {
                e.ToTable("Carts");
                e.HasKey(x => x.Cart_Id);

                // User (1) --- (N) Cart
                e.HasOne(x => x.User).WithMany(u => u.Carts)
                    .HasForeignKey(x => x.User_Id).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CartItem>(e =>
            {
                e.ToTable("CartItems");
                e.HasKey(x => x.CartItem_Id);

                // Cart (1) --- (N) CartItem: позиции удаляются вместе с корзиной
                e.HasOne(x => x.Cart).WithMany(c => c.Items)
                    .HasForeignKey(x => x.Cart_Id).OnDelete(DeleteBehavior.Cascade);

                // Product (1) --- (N) CartItem
                e.HasOne(x => x.Product).WithMany(p => p.CartItems)
                    .HasForeignKey(x => x.Product_Id).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Order>(e =>
            {
                e.ToTable("Orders");
                e.HasKey(x => x.Order_Id);

                // User (1) --- (N) Order
                e.HasOne(x => x.User).WithMany(u => u.Orders)
                    .HasForeignKey(x => x.User_Id).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OrderItem>(e =>
            {
                e.ToTable("OrderItems");
                e.HasKey(x => x.OrderItem_Id);
                e.Property(x => x.Price).HasColumnType("decimal(18,2)").IsRequired();

                // Order (1) --- (N) OrderItem: позиции удаляются вместе с заказом
                e.HasOne(x => x.Order).WithMany(o => o.Items)
                    .HasForeignKey(x => x.Order_Id).OnDelete(DeleteBehavior.Cascade);

                // Product (1) --- (N) OrderItem
                e.HasOne(x => x.Product).WithMany(p => p.OrderItems)
                    .HasForeignKey(x => x.Product_Id).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Reservation>(e =>
            {
                e.ToTable("Reservations");
                e.HasKey(x => x.Reservation_Id);

                // User (1) --- (N) Reservation
                e.HasOne(x => x.User).WithMany(u => u.Reservations)
                    .HasForeignKey(x => x.User_Id).OnDelete(DeleteBehavior.Restrict);

                // Product (1) --- (N) Reservation
                e.HasOne(x => x.Product).WithMany(p => p.Reservations)
                    .HasForeignKey(x => x.Product_Id).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
