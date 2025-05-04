using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;

namespace backend.Infrastructure.Data;

public static class SeedAdmin
{
    public static async Task<int> SeedAdminData(IUnitOfWork unitOfWork, string adminPasswordHash)
    {
        var adminRole = await unitOfWork.Repository<UserRole>().GetFirstOrDefaultAsync(new UserRoleSpecification("Admin"));
        if (adminRole == null)
        {
            adminRole = new UserRole
            {
                RoleName = "Admin",
                Description = "System Administrator"
            };
            unitOfWork.Repository<UserRole>().Add(adminRole);
            await unitOfWork.Complete();
        }

        var admin = await unitOfWork.Repository<Admin>().GetFirstOrDefaultAsync(new AdminSpecification("admin@example.com"));
        if (admin == null)
        {
            admin = new Admin
            {
                Email = "admin@example.com",
                PasswordHash = adminPasswordHash,
                UserRoleId = adminRole.Id,
                FirstName = "Admin",
                LastName = "User"
            };
            unitOfWork.Repository<Admin>().Add(admin);
            return await unitOfWork.Complete();
        }

        return -1; // Admin already exists
    }
}
