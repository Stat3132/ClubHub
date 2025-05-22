using Microsoft.EntityFrameworkCore;

public class OrderServiceDBContext : DbContext
{
    public OrderServiceDBContext(DbContextOptions<OrderServiceDBContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Book> Books { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserGuid);
            entity.HasIndex(u => u.Email)
                  .IsUnique(); // Unique email constraint
            entity.Property(u => u.CreatedDate)
                  .HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure Orders
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.OrderGuid);
            entity.Property(o => o.CreatedDate)
                  .HasDefaultValueSql("GETUTCDATE()");

            // One-to-many: one User has many Orders
            entity.HasOne(o => o.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(o => o.UserGuid)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Books
        modelBuilder.Entity<Book>(entity =>
        {
            // Composite key: BookUUID and OrderGuid
            entity.HasKey(b => new { b.BookUUID, b.OrderGuid });
            entity.Property(b => b.CreatedDate)
                  .HasDefaultValueSql("GETUTCDATE()");

            // Many-to-one: many Books belong to one Order
            entity.HasOne(b => b.Order)
                  .WithMany(o => o.Books)
                  .HasForeignKey(b => b.OrderGuid)
                  .OnDelete(DeleteBehavior.Cascade);

            // Configure Price column to be decimal with two decimal places
            entity.Property(b => b.Price)
                  .HasColumnType("DECIMAL(18, 2)");  // Ensures two decimal precision
        });
    }
}
