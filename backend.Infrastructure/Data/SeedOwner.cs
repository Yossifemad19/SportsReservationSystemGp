using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backend.Core.Entities;
using backend.Core.Interfaces;

namespace backend.Repository.Data;

public class SeedOwners
{
    public static async Task<int> SeedOwnersData(IUnitOfWork unitOfWork, string hashPassword)
    {
        var ownerRepo = unitOfWork.Repository<Owner>();

        if ((await ownerRepo.GetAllAsync()).Any())
            return 0;

        var owners = new List<Owner>
        {
            new Owner
            {
                Id = 1,
                FirstName = "first",
                LastName = "owner",
                Email = "owner1@gmail.com",
                PhoneNumber = "01111885599",
                PasswordHash = hashPassword,
                UserRole = UserRole.Owner,
                IsApproved = true,
                Facilities = new List<Facility>() 
            },
            new Owner
            {
                Id = 2,
                FirstName = "second",
                LastName = "owner",
                Email = "owner2@gmail.com",
                PhoneNumber = "01111225599",
                PasswordHash = hashPassword,
                UserRole = UserRole.Owner,
                IsApproved = true,
                Facilities = new List<Facility>()
            }
        };

        foreach (var owner in owners)
        {
            ownerRepo.Add(owner); 
        }

        return await unitOfWork.CompleteAsync();
    }
}
