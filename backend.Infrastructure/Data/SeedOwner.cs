﻿using System;
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
        var ownerRepo = unitOfWork.Repository<Owner>();
        var existingOwners = await ownerRepo.GetAllAsync();
        if (existingOwners.Any())
        {
            return 0;
        }

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
        new Owner { FirstName = "Ahmed", LastName = "Hassan", Email = "ahmed.hassan@gmail.com", PhoneNumber = "01000111222", PasswordHash = ownerPasswordHash, UserRoleId = ownerRole.Id, IsApproved = true },
        new Owner { FirstName = "Mohamed", LastName = "Ali", Email = "mohamed.ali@gmail.com", PhoneNumber = "01000222333", PasswordHash = ownerPasswordHash, UserRoleId = ownerRole.Id, IsApproved = true },
        new Owner { FirstName = "Fatma", LastName = "Saad", Email = "fatma.saad@gmail.com", PhoneNumber = "01000333444", PasswordHash = ownerPasswordHash, UserRoleId = ownerRole.Id, IsApproved = true },
        new Owner { FirstName = "Omar", LastName = "Sherif", Email = "omar.sherif@gmail.com", PhoneNumber = "01000444555", PasswordHash = ownerPasswordHash, UserRoleId = ownerRole.Id, IsApproved = true },
        new Owner { FirstName = "Sara", LastName = "Adel", Email = "sara.adel@gmail.com", PhoneNumber = "01000555666", PasswordHash = ownerPasswordHash, UserRoleId = ownerRole.Id, IsApproved = true },
        new Owner { FirstName = "Youssef", LastName = "Nabil", Email = "youssef.nabil@gmail.com", PhoneNumber = "01000666777", PasswordHash = ownerPasswordHash, UserRoleId = ownerRole.Id, IsApproved = true },
        new Owner { FirstName = "Mona", LastName = "Ibrahim", Email = "mona.ibrahim@gmail.com", PhoneNumber = "01000777888", PasswordHash = ownerPasswordHash, UserRoleId = ownerRole.Id, IsApproved = true },
        new Owner { FirstName = "Khaled", LastName = "Mostafa", Email = "khaled.mostafa@gmail.com", PhoneNumber = "01000888999", PasswordHash = ownerPasswordHash, UserRoleId = ownerRole.Id, IsApproved = true },
        new Owner { FirstName = "Nada", LastName = "Hany", Email = "nada.hany@gmail.com", PhoneNumber = "01000999000", PasswordHash = ownerPasswordHash, UserRoleId = ownerRole.Id, IsApproved = true },
        new Owner { FirstName = "Tamer", LastName = "Fahmy", Email = "tamer.fahmy@gmail.com", PhoneNumber = "01001000111", PasswordHash = ownerPasswordHash, UserRoleId = ownerRole.Id, IsApproved = true },
        new Owner { FirstName = "Hana", LastName = "Khalil", Email = "hana.khalil@gmail.com", PhoneNumber = "01001111222", PasswordHash = ownerPasswordHash, UserRoleId = ownerRole.Id, IsApproved = true },
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
