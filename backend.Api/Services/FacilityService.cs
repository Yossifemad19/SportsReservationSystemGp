using AutoMapper;
using backend.Api.DTOs;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Repository.Data;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace backend.Api.Services;


public class FacilityService : IFacilityService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;

    public FacilityService(IUnitOfWork unitOfWork, IMapper mapper,IWebHostEnvironment env)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _env = env;
    }

    public async Task<FacilityDto?> GetFacilityById(int id)
    {
        var facility = await _unitOfWork.Repository<Facility>()
                                        .GetByIdAsync(id);
        
        return facility is null?null:_mapper.Map<FacilityDto>(facility);
    }

    public async Task<FacilityResponseDto> CreateFacility(FacilityDto facilityDto, string ownerId)
{
    if (facilityDto.Address == null)
    {
            return null;
    }

    decimal latitude = facilityDto.Address.Latitude;
    decimal longitude = facilityDto.Address.Longitude;

    if (!IsWithinCairo(latitude, longitude) || !IsWithinGiza(latitude, longitude))
    {
        return new FacilityResponseDto { Message = "Facility location must be inside Cairo or Giza" };
    }

        var imagesFolder = Path.Combine(_env.WebRootPath,"images/facilities");
    if (!Directory.Exists(imagesFolder)) 
        Directory.CreateDirectory(imagesFolder);
    
    var imageFile = $"{Guid.NewGuid()}-{facilityDto.Image.FileName}";
    var relativePath = Path.Combine("images/facilities", imageFile);
    var filePath = Path.Combine(imagesFolder, imageFile);

    using(var fileStream =new FileStream(filePath, FileMode.Create))
    {
        await facilityDto.Image.CopyToAsync(fileStream);
    }

    
    var facility = _mapper.Map<Facility>(facilityDto);
    facility.OwnerId = int.Parse(ownerId);
    facility.ImageUrl = relativePath;
    _unitOfWork.Repository<Facility>().Add(facility);
    var result = await _unitOfWork.Complete();

    return result > 0 
        ? new FacilityResponseDto { Message = "Facility created successfully.", Data = _mapper.Map<FacilityDto>(facility) } 
        : null;
    }


    private bool IsWithinCairo(decimal latitude, decimal longitude)
    {
        return latitude >= 29.8m && latitude <= 30.2m &&
               longitude >= 31.1m && longitude <= 31.5m;
    }

    private bool IsWithinGiza(decimal latitude, decimal longitude)
    {
        return latitude >= 29.85m && latitude <= 30.1m &&
               longitude >= 31.0m && longitude <= 31.3m;
    }





    // public async Task<bool> UpdateFacility(FacilityDto facilityDto)
    // {
    //     var existingFacility = await _unitOfWork.Repository<Facility>()
    //                                             .GetByIdAsync(facilityDto.Id);
    //
    //     if (existingFacility == null)
    //         return false;
    //
    //
    //     // if you want to do The mapping manually 
    //
    //     existingFacility.Name = facilityDto.Name;
    //
    //    
    //     existingFacility.Address = new Address
    //     {
    //         StreetAddress = facilityDto.Address.StreetAddress,
    //         City = facilityDto.Address.City,
    //         Latitude = facilityDto.Address.Longitude,
    //         Longitude = facilityDto.Address.Longitude,
    //         
    //     };
    //
    //     _unitOfWork.Repository<Facility>().Update(existingFacility);
    //     await _unitOfWork.CompleteAsync();
    //
    //     return true;
    // }

    public async Task<bool> DeleteFacility(int id)
    {
        var facility = await _unitOfWork.Repository<Facility>().GetByIdAsync(id);

        if (facility == null)
            return false;

        _unitOfWork.Repository<Facility>().Remove(facility);
        

        return await _unitOfWork.Complete()>0?true:false;
    }

    public async Task<List<FacilityDto>> GetAllFacilities()
    {
        var facilities = await _unitOfWork.Repository<Facility>().GetAllIncludingAsync(f => f.Address); 

        return _mapper.Map<List<FacilityDto>>(facilities);
    }

}
