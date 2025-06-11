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
            return 0;

        var imageFolder = Path.Combine("images", "sports");

        var sports = new List<Sport>
           {
               new Sport { Name = "Football", ImageUrl = Path.Combine(imageFolder, "Football_image.jpg") },
               new Sport { Name = "Basketball", ImageUrl = Path.Combine(imageFolder, "Basketball_image.jpg") },
               new Sport { Name = "Tennis", ImageUrl = Path.Combine(imageFolder, "Tennis_image.jpg") },
               new Sport { Name = "Volleyball", ImageUrl = Path.Combine(imageFolder, "Volleyball_image.jpg") },
               new Sport { Name = "Padel", ImageUrl = Path.Combine(imageFolder, "Padel_image.jpg") }
           };

        foreach (var sport in sports)
        {
            sportRepository.Add(sport);
        }

        return await unitOfWork.Complete();
    }
}

