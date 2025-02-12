using backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Repository.Data.EntitiesConfigurations;

public class UserCredentialConfigurations:IEntityTypeConfiguration<UserCredential>
{
    public void Configure(EntityTypeBuilder<UserCredential> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x=>x.Email).IsRequired()
            .HasColumnType("varchar(20)")
            ;
        builder.HasIndex(x=>x.Email).IsUnique();
        
        builder.Property(x => x.PasswordHash).IsRequired().HasColumnType("varchar(64)");
        builder.Property(x=>x.CreatedAt).IsRequired()
            .HasColumnType("timestamp without time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        
        // Relations
        builder.HasOne(x=>x.UserProfile).WithOne(x=>x.UserCredential);
        
    }
}