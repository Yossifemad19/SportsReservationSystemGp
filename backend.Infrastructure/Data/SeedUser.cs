using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;

namespace backend.Infrastructure.Data;

public class SeedUsers
{
    public static async Task<int> SeedUsersData(IUnitOfWork unitOfWork, string userPasswordHash)
    {
        var userRepo = unitOfWork.Repository<User>();

        
        var existingUsers = await userRepo.GetAllAsync();
        if (existingUsers.Any())
        {
            return 0; 
        }

        
        var userRole = await unitOfWork.Repository<UserRole>().GetFirstOrDefaultAsync(new UserRoleSpecification("Customer"));
        if (userRole == null)
        {
            userRole = new UserRole
            {
                RoleName = "Customer",
                Description = "Customer"
            };
            unitOfWork.Repository<UserRole>().Add(userRole);
            await unitOfWork.Complete(); // Commit role creation
        }

        // Seed users
        var users = new List<User>
    {
        new User
        {
            UserName = "user1",
            FirstName = "first",
            LastName = "user",
            Email = "user1@gmail.com",
            PhoneNumber = "01111885599",
            PasswordHash = userPasswordHash,
            UserRoleId = userRole.Id,
            PlayerProfile = new PlayerProfile
            {
                SkillLevel = 5,
                PreferredPlayingStyle = "Balanced",
                PreferredSports = new List<string>(),
                PreferredPlayingTimes = new List<string>(),
                PrefersCompetitivePlay = true,
                PrefersCasualPlay = true,
                PreferredTeamSize = 2,
                WeeklyAvailability = new Dictionary<DayOfWeek, List<TimeSpan>>(),
                SportSpecificSkills = new Dictionary<string, int>(),
                FrequentPartners = new List<string>(),
                BlockedPlayers = new List<string>(),
                MatchesPlayed = 0,
                MatchesWon = 0,
                LastUpdated = DateTime.UtcNow
            }
        },
        new User
        {
            UserName = "user2",
            FirstName = "second",
            LastName = "user",
            Email = "user2@gmail.com",
            PhoneNumber = "01111225599",
            PasswordHash = userPasswordHash,
            UserRoleId = userRole.Id,
            PlayerProfile = new PlayerProfile
            {
                SkillLevel = 5,
                PreferredPlayingStyle = "Balanced",
                PreferredSports = new List<string>(),
                PreferredPlayingTimes = new List<string>(),
                PrefersCompetitivePlay = true,
                PrefersCasualPlay = true,
                PreferredTeamSize = 2,
                WeeklyAvailability = new Dictionary<DayOfWeek, List<TimeSpan>>(),
                SportSpecificSkills = new Dictionary<string, int>(),
                FrequentPartners = new List<string>(),
                BlockedPlayers = new List<string>(),
                MatchesPlayed = 0,
                MatchesWon = 0,
                LastUpdated = DateTime.UtcNow
            }
        }
    };

        foreach (var user in users)
        {
            userRepo.Add(user);
        }

        return await unitOfWork.Complete();
    }

}
