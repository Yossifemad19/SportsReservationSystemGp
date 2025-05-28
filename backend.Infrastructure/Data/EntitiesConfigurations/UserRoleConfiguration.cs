using backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Data.EntitiesConfigurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(ur => ur.Id);
            
            builder.Property(ur => ur.RoleName)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(ur => ur.Description)
                .HasMaxLength(200);
            
            builder.HasIndex(ur => ur.RoleName)
                .IsUnique();
        }
    }
} 