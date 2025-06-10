using System;
using System.Collections.Generic;
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
            await unitOfWork.Complete();
        }

        var random = new Random();
        var firstNames = new[] { "Ahmed", "Mohamed", "Sara", "Yasmine", "Omar", "Laila", "Khaled", "Mona", "Tarek", "Dina" };
        var lastNames = new[] { "Hassan", "Ali", "Saeed", "Mahmoud", "Fahmy", "Nasser", "Ibrahim", "Hany", "Salah", "Fathy" };
        var playingStyles = new[] { "Aggressive", "Defensive", "Balanced", "Attacking" };

        var usernames = new HashSet<string>();
        var users = new List<User>();

        for (int i = 0; i < 20; i++)
        {
            string firstName = firstNames[random.Next(firstNames.Length)];
            string lastName = lastNames[random.Next(lastNames.Length)];
            int number = random.Next(10, 100);

            string baseUsername = $"{firstName.ToLower()}.{lastName.ToLower()}";
            string username = baseUsername;
            string email = $"{baseUsername}{number}@gmail.com";

            
            while (usernames.Contains(username))
            {
                number = random.Next(10, 100);
                username = $"{baseUsername}{number}";
                email = $"{baseUsername}{number}@gmail.com";
            }
            usernames.Add(username);

            var preferredSports = new List<string>();
            var sportsCount = random.Next(1, 4);
            for (int s = 0; s < sportsCount; s++)
            {
                var sport = new[] { "Football", "Basketball", "Tennis", "Volleyball", "Padel" }[random.Next(5)];
                if (!preferredSports.Contains(sport))
                    preferredSports.Add(sport);
            }

            users.Add(new User
            {
                UserName = username,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = $"0111{random.Next(1000000, 9999999)}",
                PasswordHash = userPasswordHash,
                UserRoleId = userRole.Id,
                PlayerProfile = new PlayerProfile
                {
                    SkillLevel = random.Next(1, 11),
                    PreferredPlayingStyle = playingStyles[random.Next(playingStyles.Length)],
                    PreferredSports = preferredSports,
                    PreferredPlayingTimes = new List<string> { "Evenings", "Weekends" },
                    PrefersCompetitivePlay = random.Next(0, 2) == 1,
                    PrefersCasualPlay = random.Next(0, 2) == 1,
                    PreferredTeamSize = random.Next(1, 6),
                    WeeklyAvailability = new Dictionary<DayOfWeek, List<TimeSpan>>
                    {
                        { DayOfWeek.Monday, new List<TimeSpan> { new TimeSpan(18, 0, 0), new TimeSpan(20, 0, 0) } },
                        { DayOfWeek.Wednesday, new List<TimeSpan> { new TimeSpan(18, 0, 0) } },
                        { DayOfWeek.Friday, new List<TimeSpan> { new TimeSpan(10, 0, 0), new TimeSpan(15, 0, 0) } }
                    },
                    SportSpecificSkills = new Dictionary<string, int>(),
                    FrequentPartners = new List<string>(),
                    BlockedPlayers = new List<string>(),
                    MatchesPlayed = random.Next(0, 50),
                    MatchesWon = random.Next(0, 40),
                    LastUpdated = DateTime.UtcNow
                }
            });
        }

        foreach (var user in users)
        {
            userRepo.Add(user);
        }

        return await unitOfWork.Complete();
    }
}
