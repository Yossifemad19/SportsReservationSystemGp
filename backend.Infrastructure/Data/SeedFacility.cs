using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backend.Core.Entities;
using backend.Core.Interfaces;

namespace backend.Repository.Data;

public static class SeedFacilities
{
    public static async Task<int> SeedFacilitiesData(IUnitOfWork unitOfWork)
    {
        var facilityRepository = unitOfWork.Repository<Facility>();
        var ownerRepository = unitOfWork.Repository<Owner>(); 
        var addressRepository = unitOfWork.Repository<Address>(); 

        
        var existingFacilities = await facilityRepository.GetAllAsync();
        if (existingFacilities.Any())
        {
            return 0; 
        }

        
        var owners = await ownerRepository.GetAllAsync();
        if (!owners.Any())
        {
            return -1; 
        }

        
        var ownersList = owners.ToList();

        
        var facilities = new List<Facility>
        {
            new Facility
            {
                Name = "Facility 1",
                OwnerId = ownersList[0].Id,  
                ImageUrl = "images/facilities/400d0b97-5b45-4c74-8c5e-3487a48a2f61-download (2).jpeg",
                OpeningTime = new TimeSpan(8,0,0),
                ClosingTime = new TimeSpan(22,0,0),
                
                Address = new Address
                {
                    StreetAddress = "Lebanon St",
                    City = "Cairo",
                    Latitude = 30.0339m,  
                    Longitude = 31.2336m  
                },
            },
            new Facility
            {
                Name = "Facility 2",
                OwnerId = ownersList[1].Id,  
                ImageUrl = "images/facilities/400d0b97-5b45-4c74-8c5e-3487a48a2f61-download (2).jpeg",
                OpeningTime = new TimeSpan(8,0,0),
                ClosingTime = new TimeSpan(22,0,0),
                Address = new Address
                {
                    StreetAddress = "Tahrir St",
                    City = "Cairo",
                    Latitude = 30.0444m,  
                    Longitude = 31.2357m  
                },
            },
            new Facility
            {
                Name = "Facility 3",
                OwnerId = ownersList[0].Id,  
                ImageUrl = "images/facilities/400d0b97-5b45-4c74-8c5e-3487a48a2f61-download (2).jpeg",
                OpeningTime = new TimeSpan(8,0,0),
                ClosingTime = new TimeSpan(22,0,0),
                Address = new Address
                {
                    StreetAddress = "El Nasr St",
                    City = "Cairo",
                    Latitude = 30.0632m,  
                    Longitude = 31.2481m  
                },
            },
            new Facility
            {
                Name = "Facility 4",
                OwnerId = ownersList[1].Id,  
                ImageUrl = "images/facilities/400d0b97-5b45-4c74-8c5e-3487a48a2f61-download (2).jpeg",
                OpeningTime = new TimeSpan(8,0,0),
                ClosingTime = new TimeSpan(22,0,0),
                Address = new Address
                {
                    StreetAddress = "Ahmed Oraby St",
                    City = "Cairo",
                    Latitude = 30.0615m,  
                    Longitude = 31.2274m  
                },
                
            },
            new Facility
            {
                Name = "Facility 5",
                OwnerId = ownersList[0].Id,  
                ImageUrl = "images/facilities/400d0b97-5b45-4c74-8c5e-3487a48a2f61-download (2).jpeg",
                OpeningTime = new TimeSpan(8,0,0),
                ClosingTime = new TimeSpan(22,0,0),
                Address = new Address
                {
                    StreetAddress = "Dokki St",
                    City = "Cairo",
                    Latitude = 30.0435m,  
                    Longitude = 31.2475m  
                },
            }
        };

        
        foreach (var facility in facilities)
        {
            facilityRepository.Add(facility);
        }

        return await unitOfWork.Complete();
    }
}


