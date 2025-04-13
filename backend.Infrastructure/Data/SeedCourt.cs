using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backend.Core.Entities;
using backend.Core.Interfaces;

namespace backend.Repository.Data;

public static class SeedCourts
{
    public static async Task<int> SeedCourtsData(IUnitOfWork unitOfWork)
    {
        var courtRepository = unitOfWork.Repository<Court>();
        var facilityRepository = unitOfWork.Repository<Facility>();
        var sportRepository = unitOfWork.Repository<Sport>();

        
        var existingCourts = await courtRepository.GetAllAsync();
        if (existingCourts.Any())
        {
            return 0; 
        }

        
        var facilities = (await facilityRepository.GetAllAsync()).ToList();
        var sports = (await sportRepository.GetAllAsync()).ToList(); 

        if (!facilities.Any() || !sports.Any())
        {
            return -1; 
        }

        
        var courts = new List<Court>
        {
            new Court
            {
                Name = "Court 1",
                FacilityId = facilities[0].Id,  
                SportId = sports[0].Id,  
                Capacity = 10,
                PricePerHour = 50.00m
            },
            new Court
            {
                Name = "Court 2",
                FacilityId = facilities[1].Id,  
                SportId = sports[0].Id,  
                Capacity = 10,
                PricePerHour = 60.00m
            },
            new Court
            {
                Name = "Court 3",
                FacilityId = facilities[2].Id,  
                SportId = sports[0].Id,  
                Capacity = 10,
                PricePerHour = 40.00m
            }
        };

        
        foreach (var court in courts)
        {
            courtRepository.Add(court);
        }

        return await unitOfWork.CompleteAsync(); 
    }
}

