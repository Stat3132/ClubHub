using Microsoft.EntityFrameworkCore;
using UserService;

namespace UserService.Models
{
    public class ClubHubDBContext : DbContext
    {
        public ClubHubDBContext(DbContextOptions<ClubHubDBContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Club> Clubs { get; set; } = null!;
        public DbSet<UserClub> UserClubs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.userID);
                entity.HasIndex(u => u.email)
                      .IsUnique(); // Unique email constraint

                entity.Property(u => u.role)
                      .HasConversion<string>(); // Store enum as string
            });

            // Configure Clubs
            modelBuilder.Entity<Club>(entity =>
            {
                entity.HasKey(c => c.clubID);

                // Relationships with User (President and Advisor)
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(c => c.presidentID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(c => c.advisorID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure UserClub (Junction Table)
            modelBuilder.Entity<UserClub>(entity =>
            {
                entity.HasKey(uc => uc.userClubID);

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(uc => uc.userID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Club>()
                      .WithMany()
                      .HasForeignKey(uc => uc.clubID)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
