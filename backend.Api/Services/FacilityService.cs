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

    public FacilityService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;

    }

    public async Task<FacilityDto?> GetFacilityById(int id)
    {
        var facility = await _unitOfWork.Repository<Facility>()
                                        .GetByIdAsync(id);
        
        return facility is null?null:_mapper.Map<FacilityDto>(facility);
    }

    public async Task<FacilityDto> CreateFacility(FacilityDto facilityDto,string ownerId)
    {
        var facility = _mapper.Map<Facility>(facilityDto);  
        
        facility.OwnerId = int.Parse(ownerId);
        
        _unitOfWork.Repository<Facility>().Add(facility);

        var result = await _unitOfWork.CompleteAsync();
        
        return result>0 ? _mapper.Map<FacilityDto>(facility):null;  
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
        

        return await _unitOfWork.CompleteAsync()>0?true:false;
    }
}
