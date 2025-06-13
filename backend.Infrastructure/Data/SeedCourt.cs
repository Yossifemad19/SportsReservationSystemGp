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

        var courts = new List<Court>();

        foreach (var facility in facilities)
        {
            // Per-facility sport counters
            var facilityCourtCounters = new Dictionary<string, int>
            {
                { "Football", 1 },
                { "Basketball", 1 },
                { "Tennis", 1 },
                { "Volleyball", 1 },
                { "Padel", 1 }
            };

            // Add one court per sport
            foreach (var sport in sports)
            {
                var sportName = sport.Name;
                var courtNumber = facilityCourtCounters[sportName]++;
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

            
            var football = sports.First(s => s.Name == "Football");
            var extraFootballNumber = facilityCourtCounters["Football"]++;
            var extraFootballName = $"Football Court {extraFootballNumber}";

            courts.Add(new Court
            {
                Name = extraFootballName,
                FacilityId = facility.Id,
                SportId = football.Id,
                Capacity = 10,
                PricePerHour = 200.00m
            });
        }

        foreach (var court in courts)
            courtRepository.Add(court);

        return await unitOfWork.Complete();
    }
}






