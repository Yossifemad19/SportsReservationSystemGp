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
            return 0;

        var facilities = (await facilityRepository.GetAllAsync()).ToList();
        var sports = (await sportRepository.GetAllAsync()).ToList();

        if (!facilities.Any() || !sports.Any())
            return -1;

        var random = new Random();
        var courts = new List<Court>();

        var courtCounters = new Dictionary<string, int>
    {
        { "Football", 1 },
        { "Basketball", 1 },
        { "Tennis", 1 },
        { "Volleyball", 1 },
        { "Padel", 1 }
    };

        // Step 1: Ensure each facility has at least one court
        foreach (var facility in facilities)
        {
            var sport = sports[random.Next(sports.Count)];
            var sportName = sport.Name;
            var courtNumber = courtCounters[sportName]++;
            var courtName = $"{sportName} Court {courtNumber}";

            decimal price = sportName switch
            {
                "Football" => 200.00m,
                "Basketball" => 200.00m,
                "Tennis" => 200.00m,
                "Volleyball" => 150.00m,
                "Padel" => 300.00m,
                _ => 200.00m
            };

            int capacity = sportName switch
            {
                "Football" => 10,
                "Basketball" => 10,
                "Tennis" => 2,
                "Volleyball" => 6,
                "Padel" => 4,
                _ => 10
            };

            courts.Add(new Court
            {
                Name = courtName,
                FacilityId = facility.Id,
                SportId = sport.Id,
                Capacity = capacity,
                PricePerHour = price
            });
        }

        
        while (courts.Count < 30)
        {
            var facility = facilities[random.Next(facilities.Count)];

            Sport sport;
            int chance = random.Next(100);
            if (chance < 50)
                sport = sports.First(s => s.Name == "Football");
            else
                sport = sports[random.Next(sports.Count)];

            var sportName = sport.Name;
            var courtNumber = courtCounters[sportName]++;
            var courtName = $"{sportName} Court {courtNumber}";

            decimal price = sportName switch
            {
                "Football" => 200.00m,
                "Basketball" => 200.00m,
                "Tennis" => 200.00m,
                "Volleyball" => 150.00m,
                "Padel" => 300.00m,
                _ => 200.00m
            };

            int capacity = sportName switch
            {
                "Football" => 10,
                "Basketball" => 10,
                "Tennis" => 2,
                "Volleyball" => 6,
                "Padel" => 4,
                _ => 10
            };

            courts.Add(new Court
            {
                Name = courtName,
                FacilityId = facility.Id,
                SportId = sport.Id,
                Capacity = capacity,
                PricePerHour = price
            });
        }

        foreach (var court in courts)
            courtRepository.Add(court);

        return await unitOfWork.Complete();
    }

}





