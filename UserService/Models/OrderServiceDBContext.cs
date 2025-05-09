using Microsoft.EntityFrameworkCore;

public class OrderServiceDBContext : DbContext
{
    public OrderServiceDBContext(DbContextOptions<OrderServiceDBContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Food> Foods { get; set; } = null!;

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

        // Configure Foods
        modelBuilder.Entity<Food>(entity =>
        {
            // Composite key: FoodID and OrderGuid
            entity.HasKey(b => new { b.FoodID, b.OrderGuid });
            entity.Property(b => b.CreatedDate)
                    .HasDefaultValueSql("GETUTCDATE()");

            // Many-to-one: many Foods belong to one Order
            entity.HasOne(b => b.Order)
                    .WithMany(o => o.Foods)
                    .HasForeignKey(b => b.OrderGuid)
                    .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
