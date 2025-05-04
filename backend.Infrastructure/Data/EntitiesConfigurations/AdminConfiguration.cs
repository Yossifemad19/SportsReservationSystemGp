using backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Repository.Data.EntitiesConfigurations;

public class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("Admins");
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.FirstName).IsRequired();
        builder.Property(a => a.LastName).IsRequired();
        builder.Property(a => a.Email).IsRequired();
        builder.Property(a => a.PasswordHash).IsRequired();
        
        builder.HasIndex(a => a.Email).IsUnique();
        
        // Configure UserRole as a navigation property
        builder.HasOne(a => a.UserRole)
            .WithMany()
            .HasForeignKey(a => a.UserRoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
} 