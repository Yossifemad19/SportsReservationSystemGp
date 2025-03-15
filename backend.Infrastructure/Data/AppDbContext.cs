using backend.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Data;

public class AppDbContext: DbContext
{
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public AppDbContext() : base()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        modelBuilder.Entity<Sport>().HasMany(s=>s.Courts)
            .WithOne(c=>c.Sport).HasForeignKey(c=>c.SportId);
        
        modelBuilder.Entity<Facility>().HasMany(f=>f.Courts)
            .WithOne(c=>c.Facility).HasForeignKey(c=>c.FacilityId);

        modelBuilder.Entity<Booking>().HasOne(b => b.User)
            .WithMany(u => u.Bookings).HasForeignKey(b=>b.UserId);
        
        modelBuilder.Entity<Booking>().HasOne(b=>b.Court)
            .WithMany(c=>c.Bookings).HasForeignKey(b=>b.CourtId);
        
        modelBuilder.Entity<Booking>().Property(b=>b.status).HasConversion(os => os.ToString(), os => (BookingStatus)Enum.Parse(typeof(BookingStatus),os));
        
        modelBuilder.Entity<Owner>().HasMany(o => o.Facilities)
            .WithOne(f => f.Owner)
            .HasForeignKey(f => f.OwnerId);

        modelBuilder.Entity<User>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(a => a.UserRole)
            .HasConversion<string>();

        modelBuilder.Entity<Admin>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<Admin>()
            .Property(a => a.UserRole)
            .HasConversion<string>();

        modelBuilder.Entity<Owner>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<Admin>()
            .Property(a => a.UserRole)
            .HasConversion<string>();

        modelBuilder.Entity<Owner>()
            .Property(a => a.UserRole)
            .HasConversion<string>();

       


    }

    public DbSet<User> Users { get; set; }
    public DbSet<Owner> Owners { get; set; }
    public DbSet<Admin> Admins { get; set; }

    public DbSet<Address> Addresses { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Court> Courts { get; set; }
    public DbSet<Facility> Facilities { get; set; }
    public DbSet<Sport> Sports { get; set; }

    
}