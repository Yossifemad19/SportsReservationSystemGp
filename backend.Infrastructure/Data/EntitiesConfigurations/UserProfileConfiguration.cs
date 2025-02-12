using backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Repository.Data.EntitiesConfigurations;


public class UserProfileConfiguration: IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).IsRequired()
            .HasColumnType("varchar(10)");
        builder.Property(x => x.LastName).IsRequired()
            .HasColumnType("varchar(10)");
        builder.Property(x=>x.PhoneNumber).HasColumnType("varchar(11)");
        builder.Property(x=>x.Address).HasColumnType("varchar(20)");
        
        // Relations
        builder.HasOne(x=>x.UserCredential).WithOne(x=>x.UserProfile)
            .HasForeignKey<UserProfile>(x=>x.UserCredentialId);
    }
}