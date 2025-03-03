using backend.Core.Entities;

namespace backend.Repository.Data;

public class SeedAdmin
{
    public static async Task<int> SeedAdminData(AppDbContext context,string HashPassword)
    {
        if (!context.User.Any(u => u.UserRole == UserRole.Admin))
        {
            var adminUser = new User
            {
                FirstName = "admin",
                LastName = "admin",
                PasswordHash = HashPassword,
                UserRole = UserRole.Admin,
                Email = "admin@gmail.com",
            };
        
            context.User.Add(adminUser);
            
            return await context.SaveChangesAsync();
        }
        return -1;
    }
}