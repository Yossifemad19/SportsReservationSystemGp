using backend.Core.Entities;
using backend.Core.Interfaces;

namespace backend.Repository.Data;

public class SeedAdmin
{
    public static async Task<int> SeedAdminData(IUnitOfWork unitOfWork, string hashPassword)
    {
        var adminRepository = unitOfWork.Repository<Admin>();

        var existingAdmin = await adminRepository.FindAsync(u => u.UserRole == UserRole.Admin);

        if (existingAdmin == null) 
        {
            var adminUser = new Admin
            {
                FirstName = "admin",
                LastName = "admin",
                PasswordHash = hashPassword,
                UserRole = UserRole.Admin,
                Email = "admin@gmail.com",
            };

            adminRepository.Add(adminUser);

            return await unitOfWork.CompleteAsync();
        }

        return -1;
    }
}
