using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backend.Core.Entities;
using backend.Core.Interfaces;

namespace backend.Repository.Data;

public static class SeedSports
{
    public static async Task<int> SeedSportsData(IUnitOfWork unitOfWork)
    {
        var sportRepository = unitOfWork.Repository<Sport>();

        var existingSports = await sportRepository.GetAllAsync();
        if (existingSports.Any())
        {
            return 0;
        }

        var sports = new List<Sport>
        {
            new Sport { Name = "Football" },
            new Sport { Name = "Basketball" },
            new Sport { Name = "Tennis" },
            new Sport { Name = "Volleyball" },
            new Sport { Name = "Padel" }
        };

        foreach (var sport in sports)
        {
            sportRepository.Add(sport);
        }

        return await unitOfWork.CompleteAsync(); 
    }
}

