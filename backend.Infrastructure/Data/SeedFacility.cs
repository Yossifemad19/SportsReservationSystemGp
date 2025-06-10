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
        if (ownersList.Count < 20)
        {
            return -2; // Not enough owners
        }

        var facilityNames = new List<string>
    {
        "Nile Sports Complex",
        "Cairo Stars Club",
        "El Gezira Athletic Center",
        "Giza Sports Dome",
        "Tahrir Sports Arena",
        "Zamalek Youth Center",
        "Victory Sports City",
        "Heliopolis Sports Union",
        "Masr Sports Academy",
        "El Nasr Sports Hub",
        "Maadi Fitness & Sports Park",
        "Champions Sports Village",
        "New Cairo Multi-Sports Club",
        "Tagamoa Sports Hall",
        "Wadi Degla Sports Center",
        "Pyramids Athletic Club",
        "Al Qahera Sports Fields",
        "Golden Goal Arena",
        "October Sports Complex",
        "Future Athletes Center"
    };

        var addresses = new List<(string Street, string City, decimal Lat, decimal Lng)>
    {
        ("Lebanon St", "Giza", 30.0418m, 31.2046m),
        ("Tahrir St", "Cairo", 30.0444m, 31.2357m),
        ("Ahmed Oraby St", "Giza", 30.0615m, 31.2274m),
        ("El Haram St", "Giza", 29.9948m, 31.1454m),
        ("El Nasr Rd", "Cairo", 30.0632m, 31.2481m),
        ("Corniche El Nil", "Cairo", 30.0300m, 31.2300m),
        ("Gamal Abdel Nasser St", "Giza", 30.0200m, 31.2000m),
        ("Al Manial St", "Cairo", 30.0254m, 31.2234m),
        ("October 6th Bridge", "Cairo", 30.0600m, 31.2300m),
        ("Abbas El Akkad St", "Cairo", 30.0565m, 31.3352m),
        ("El Talbia St", "Giza", 29.9911m, 31.1500m),
        ("Dokki St", "Giza", 30.0347m, 31.2155m),
        ("Sphinx Sq", "Giza", 30.0424m, 31.2103m),
        ("Mohandessin St", "Giza", 30.0563m, 31.2035m),
        ("Zamalek St", "Cairo", 30.0612m, 31.2221m),
        ("Heliopolis Sq", "Cairo", 30.1004m, 31.3284m),
        ("Nasr City Main Rd", "Cairo", 30.0519m, 31.3771m),
        ("Maadi Corniche", "Cairo", 29.9633m, 31.2610m),
        ("El Rehab Gate 9", "Cairo", 30.0301m, 31.4911m),
        ("Shubra St", "Cairo", 30.0843m, 31.2474m)
    };

        var facilities = new List<Facility>();

        for (int i = 0; i < 20; i++)
        {
            var (street, city, lat, lng) = addresses[i];

            facilities.Add(new Facility
            {
                Name = facilityNames[i],
                OwnerId = ownersList[i].Id,
                ImageUrl = "images/facilities/400d0b97-5b45-4c74-8c5e-3487a48a2f61-download (2).jpeg", 
                OpeningTime = new TimeSpan(8, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0),
                Address = new Address
                {
                    StreetAddress = street,
                    City = city,
                    Latitude = lat,
                    Longitude = lng
                }
            });
        }

        foreach (var facility in facilities)
        {
            facilityRepository.Add(facility);
        }

        return await unitOfWork.Complete();
    }

}


