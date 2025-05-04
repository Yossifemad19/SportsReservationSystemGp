using backend.Core.Entities;
using backend.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Infrastructure.Data
{
    public static class SeedUserRoles
    {
        public static async Task<int> SeedRoles(IUnitOfWork unitOfWork)
        {
            var roles = new List<string> { "Admin", "Owner", "Customer" };
            int count = 0;

            foreach (var roleName in roles)
            {
                var role = await unitOfWork.Repository<UserRole>()
                    .FindAsync(r => r.RoleName == roleName);
                
                if (role == null)
                {
                    role = new UserRole
                    {
                        RoleName = roleName,
                        Description = $"{roleName} role"
                    };
                    unitOfWork.Repository<UserRole>().Add(role);
                    count++;
                }
            }

            if (count > 0)
            {
                await unitOfWork.Complete();
            }

            return count;
        }
    }
} 