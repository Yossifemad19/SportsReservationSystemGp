using backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Repository.Data.EntitiesConfigurations;


public class UserProfileConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.FirstName).IsRequired()
            .HasColumnType("varchar(10)");
        builder.Property(x => x.LastName).IsRequired()
            .HasColumnType("varchar(10)");
        builder.Property(x=>x.PhoneNumber).HasColumnType("varchar(11)");
        builder.Property(x=>x.Email).IsRequired()
            .HasColumnType("varchar(30)")
            ;
        builder.HasIndex(x=>x.Email).IsUnique();
        
        builder.Property(x => x.PasswordHash).IsRequired().HasColumnType("varchar(64)");
        
        builder.Property(x=>x.CreatedAt).IsRequired()
            .HasColumnType("timestamp without time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // Configure UserRole as a navigation property
        builder.HasOne(u => u.UserRole)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.UserRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relations
        builder.HasMany(x=>x.Bookings).WithOne(b=>b.User)
            .HasForeignKey(B=>B.UserId);


    }
}