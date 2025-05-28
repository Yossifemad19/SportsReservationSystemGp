using System.Reflection;
using backend.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public AppDbContext() : base()
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Owner> Owners { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Court> Courts { get; set; }
    public DbSet<Facility> Facilities { get; set; }
    public DbSet<Sport> Sports { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<PlayerProfile> PlayerProfiles { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<MatchPlayer> MatchPlayers { get; set; }
    public DbSet<PlayerRating> PlayerRatings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        modelBuilder.Entity<Sport>().HasMany(s => s.Courts)
            .WithOne(c => c.Sport).HasForeignKey(c => c.SportId);
        
        modelBuilder.Entity<Facility>().HasMany(f => f.Courts)
            .WithOne(c => c.Facility).HasForeignKey(c => c.FacilityId);

        modelBuilder.Entity<Booking>().HasOne(b => b.User)
            .WithMany(u => u.Bookings).HasForeignKey(b => b.UserId);
        
        modelBuilder.Entity<Booking>().HasOne(b => b.Court)
            .WithMany(c => c.Bookings).HasForeignKey(b => b.CourtId);
        
        modelBuilder.Entity<Booking>().Property(b => b.status)
            .HasConversion(os => os.ToString(), os => (BookingStatus)Enum.Parse(typeof(BookingStatus), os));
        
        modelBuilder.Entity<Owner>().HasMany(o => o.Facilities)
            .WithOne(f => f.Owner)
            .HasForeignKey(f => f.OwnerId);

        modelBuilder.Entity<User>()
            .HasIndex(a => a.Email)
            .IsUnique();

        // Configure User, Owner, and Admin indexes
        modelBuilder.Entity<Admin>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<Owner>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<UserRole>()
            .HasIndex(a => a.RoleName)
            .IsUnique();

        modelBuilder.Entity<PlayerProfile>().ToTable("PlayerProfiles");
        modelBuilder.Entity<PlayerProfile>().HasOne(p => p.User).WithOne(u => u.PlayerProfile).HasForeignKey<PlayerProfile>(p => p.UserId);
        
        // Match configuration
        modelBuilder.Entity<Match>().ToTable("Matches");
        modelBuilder.Entity<Match>()
            .HasOne(m => m.Booking)
            .WithMany()
            .HasForeignKey(m => m.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // MatchPlayer configuration
        modelBuilder.Entity<MatchPlayer>().ToTable("MatchPlayers");
        modelBuilder.Entity<MatchPlayer>()
            .HasOne(mp => mp.Match)
            .WithMany(m => m.Players)
            .HasForeignKey(mp => mp.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<MatchPlayer>()
            .HasOne(mp => mp.User)
            .WithMany()
            .HasForeignKey(mp => mp.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // PlayerRating configuration
        modelBuilder.Entity<PlayerRating>().ToTable("PlayerRatings");
        modelBuilder.Entity<PlayerRating>()
            .HasOne(pr => pr.Match)
            .WithMany(m => m.Ratings)
            .HasForeignKey(pr => pr.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlayerRating>()
            .HasOne(pr => pr.RaterUser)
            .WithMany()
            .HasForeignKey(pr => pr.RaterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PlayerRating>()
            .HasOne(pr => pr.RatedUser)
            .WithMany()
            .HasForeignKey(pr => pr.RatedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
