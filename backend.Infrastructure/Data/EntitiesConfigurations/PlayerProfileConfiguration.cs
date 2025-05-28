using backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Data.EntitiesConfigurations
{
    public class PlayerProfileConfiguration : IEntityTypeConfiguration<PlayerProfile>
    {
        public void Configure(EntityTypeBuilder<PlayerProfile> builder)
        {
            builder.HasKey(p => p.Id);
            
            builder.HasOne(p => p.User)
                .WithOne(u => u.PlayerProfile)
                .HasForeignKey<PlayerProfile>(p => p.UserId);
            
            builder.Property(p => p.SkillLevel)
                .IsRequired();
            
            builder.Property(p => p.PreferredPlayingStyle)
                .HasMaxLength(50);
            
            builder.Property(p => p.PrefersCompetitivePlay)
                .IsRequired();
            
            builder.Property(p => p.PrefersCasualPlay)
                .IsRequired();
            
            builder.Property(p => p.PreferredTeamSize)
                .IsRequired();
            
            builder.Property(p => p.LastUpdated)
                .IsRequired();
            
            // Configure JSON storage fields
            builder.Property(p => p.SportSpecificSkillsJson)
                .HasColumnType("text");
                
            builder.Property(p => p.PreferredSportsJson)
                .HasColumnType("text");
                
            builder.Property(p => p.PreferredPlayingTimesJson)
                .HasColumnType("text");
                
            builder.Property(p => p.WeeklyAvailabilityJson)
                .HasColumnType("text");
                
            builder.Property(p => p.FrequentPartnersJson)
                .HasColumnType("text");
                
            builder.Property(p => p.BlockedPlayersJson)
                .HasColumnType("text");
        }
    }
} 