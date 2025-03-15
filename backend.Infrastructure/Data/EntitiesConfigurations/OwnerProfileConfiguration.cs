using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend.Repository.Data.EntitiesConfigurations;

using backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        
        builder.ToTable("Owners");

        
        builder.HasKey(o => o.Id);

        
        builder.Property(o => o.FirstName).IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.LastName).IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Email).IsRequired()
            .HasMaxLength(255);

        builder.Property(o => o.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(o => o.PasswordHash)
            .IsRequired();

        builder.Property(o => o.UserRole)
            .IsRequired();

        builder.Property(o => o.IsApproved)
            .HasDefaultValue(false);


        builder.HasMany(o => o.Facilities).WithOne(f => f.Owner)
            .HasForeignKey(f => f.OwnerId);
            
    }
}

