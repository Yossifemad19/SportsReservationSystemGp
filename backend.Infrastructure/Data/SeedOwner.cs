using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;

namespace backend.Infrastructure.Data;

public class SeedOwners
{
    public static async Task<int> SeedOwnersData(IUnitOfWork unitOfWork, string ownerPasswordHash)
    {
        var ownerRole = await unitOfWork.Repository<UserRole>().GetFirstOrDefaultAsync(new UserRoleSpecification("Owner"));
        if (ownerRole == null)
        {
            ownerRole = new UserRole
            {
                RoleName = "Owner",
                Description = "Facility Owner"
            };
            unitOfWork.Repository<UserRole>().Add(ownerRole);
            await unitOfWork.Complete();
        }

        var owners = new List<Owner>
        {
            new Owner
            {   
                FirstName = "first",
                LastName = "owner",
                Email = "owner1@example.com",
                PhoneNumber = "01111885599",
                PasswordHash = ownerPasswordHash,
                UserRoleId = ownerRole.Id,
                IsApproved = true
            },
            new Owner
            {
                PhoneNumber = "01111225599",
                Email = "owner2@example.com",
                PasswordHash = ownerPasswordHash,
                UserRoleId = ownerRole.Id,
                FirstName = "Owner",
                LastName = "Two",
                IsApproved = true
            }
        };
                

        foreach (var owner in owners)
        {
            var existingOwner = await unitOfWork.Repository<Owner>().GetFirstOrDefaultAsync(new OwnerSpecification(owner.Email));
            if (existingOwner == null)
            {
                unitOfWork.Repository<Owner>().Add(owner);
            }
        }

        return await unitOfWork.Complete();
    }
}
