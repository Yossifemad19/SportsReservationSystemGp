using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backend.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Data.EntitiesConfigurations;


public class OwnerProfileConfiguration : IEntityTypeConfiguration<OwnerProfile>
{
    public void Configure(EntityTypeBuilder<OwnerProfile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).IsRequired()
            .HasColumnType("varchar(10)");
        builder.Property(x => x.LastName).IsRequired()
            .HasColumnType("varchar(10)");
        builder.Property(x => x.PhoneNumber).IsRequired().HasColumnType("varchar(11)");
        builder.Property(x => x.FacilitiesLocation).IsRequired().HasColumnType("varchar(50)");
        builder.Property(x => x.FacilitiesNumber).IsRequired().HasColumnType("varchar(20)");

        // Relations
        builder.HasOne(x => x.UserCredential).WithOne(x => x.OwnerProfile)
            .HasForeignKey<OwnerProfile>(x => x.UserCredentialId);
    }
}

