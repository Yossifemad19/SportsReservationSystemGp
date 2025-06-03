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
        var userRole = await unitOfWork.Repository<UserRole>().GetFirstOrDefaultAsync(new UserRoleSpecification("User"));
        if (userRole == null)
        {
            userRole = new UserRole
            {
                RoleName = "User",
                Description = "User"
            };
            unitOfWork.Repository<UserRole>().Add(userRole);
            await unitOfWork.Complete();
        }

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
           },
           new User
           {
               
               UserName = "user2",
               PhoneNumber = "01111225599",
               Email = "user2@gmail.com",
               PasswordHash = userPasswordHash,
               UserRoleId = userRole.Id,
               FirstName = "second",
               LastName = "user",
           }
       };

        foreach (var user in users)
        {
            var existingUser = await unitOfWork.Repository<User>().GetFirstOrDefaultAsync(new UserSpecification(user.Email));
            if (existingUser == null)
            {
                unitOfWork.Repository<User>().Add(user);
            }
        }

        return await unitOfWork.Complete();
    }
}
