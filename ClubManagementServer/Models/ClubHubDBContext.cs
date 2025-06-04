using Microsoft.EntityFrameworkCore;
using ClubManagementServer;

namespace ClubManagementServer.Models
{
    public class ClubHubDBContext : DbContext
    {
        public ClubHubDBContext(DbContextOptions<ClubHubDBContext> options)
            : base(options)
        {
        }

        public DbSet<User> user { get; set; } = null!;
        public DbSet<Club> club { get; set; } = null!;
        public DbSet<UserClub> userclub { get; set; } = null!;
        public DbSet<ClubJoinRequest> club_join_request { get; set; } = null!;
        public DbSet<ClubCreateRequest> club_create_request { get; set; } = null!;


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

            // Configure ClubJoinRequest
            modelBuilder.Entity<ClubJoinRequest>(entity =>
            {
                entity.HasKey(jr => jr.JoinRequestID);

                entity.Property(jr => jr.StudentName).IsRequired();
                entity.Property(jr => jr.StudentEmail).IsRequired();
                entity.Property(jr => jr.ReasonToJoin).IsRequired();

                entity.HasOne<Club>()
                      .WithMany()
                      .HasForeignKey(jr => jr.ClubID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ClubCreateRequest
            modelBuilder.Entity<ClubCreateRequest>(entity =>
            {
                entity.HasKey(cr => cr.CreateRequestID);

                entity.Property(cr => cr.ClubName).IsRequired();
                entity.Property(cr => cr.ClubDeclaration).IsRequired();
                entity.Property(cr => cr.StudentName).IsRequired();
                entity.Property(cr => cr.StudentEmail).IsRequired();
                entity.Property(cr => cr.ReasonToCreate).IsRequired();
            });
        }
    }
}
