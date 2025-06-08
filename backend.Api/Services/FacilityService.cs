using AutoMapper;
using backend.Api.DTOs;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Api.Services;

public class FacilityService : IFacilityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;

    public FacilityService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment env)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _env = env;
    }

    public async Task<ServiceResult<FacilityDto?>> GetFacilityById(int id)
    {
        var facility = await _unitOfWork.Repository<Facility>()
            .FindAsync(f => f.Id == id, f => f.Address);

        if (facility == null)
            return ServiceResult<FacilityDto?>.Fail("Facility not found.");

        return ServiceResult<FacilityDto?>.Ok(_mapper.Map<FacilityDto>(facility));
    }

    public async Task<ServiceResult<FacilityDto>> CreateFacility(FacilityDto facilityDto, string ownerId)
    {
        if (facilityDto.Address == null)
            return ServiceResult<FacilityDto>.Fail("Address is required.");

        decimal lat = facilityDto.Address.Latitude;
        decimal lng = facilityDto.Address.Longitude;

        if (!IsWithinCairo(lat, lng) && !IsWithinGiza(lat, lng))
            return ServiceResult<FacilityDto>.Fail("Facility location must be inside Cairo or Giza.");

        var imagesFolder = Path.Combine(_env.WebRootPath, "images/facilities");
        if (!Directory.Exists(imagesFolder))
            Directory.CreateDirectory(imagesFolder);

        var imageFile = $"{Guid.NewGuid()}-{facilityDto.Image.FileName}";
        var relativePath = Path.Combine("images/facilities", imageFile);
        var filePath = Path.Combine(imagesFolder, imageFile);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await facilityDto.Image.CopyToAsync(fileStream);
        }

        var facility = _mapper.Map<Facility>(facilityDto);
        facility.OwnerId = int.Parse(ownerId);
        facility.ImageUrl = relativePath;

        _unitOfWork.Repository<Facility>().Add(facility);
        var result = await _unitOfWork.Complete();

        if (result <= 0)
            return ServiceResult<FacilityDto>.Fail("Failed to create facility.");

        return ServiceResult<FacilityDto>.Ok(_mapper.Map<FacilityDto>(facility), "Facility created successfully.");
    }

    public async Task<ServiceResult<FacilityDto>> UpdateFacility(FacilityDto facilityDto, string ownerId)
    {
        var facility = await _unitOfWork.Repository<Facility>().FindAsync(
            f => f.Id == facilityDto.Id,
            f => f.Address
        );

        if (facility == null)
            return ServiceResult<FacilityDto>.Fail("Facility not found.");

        if (facility.OwnerId.ToString() != ownerId)
            return ServiceResult<FacilityDto>.Fail("You do not own this facility.");

        var lat = facilityDto.Address.Latitude;
        var lng = facilityDto.Address.Longitude;

        if (!IsWithinCairo(lat, lng) && !IsWithinGiza(lat, lng))
            return ServiceResult<FacilityDto>.Fail("Facility location must be inside Cairo or Giza.");

        _mapper.Map(facilityDto, facility);
        facility.OwnerId = int.Parse(ownerId);

        if (facilityDto.Image != null)
        {
            var imagesFolder = Path.Combine(_env.WebRootPath, "images/facilities");
            if (!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);

            var imageFile = $"{Guid.NewGuid()}-{facilityDto.Image.FileName}";
            var relativePath = Path.Combine("images/facilities", imageFile);
            var filePath = Path.Combine(imagesFolder, imageFile);

            using var stream = new FileStream(filePath, FileMode.Create);
            await facilityDto.Image.CopyToAsync(stream);

            facility.ImageUrl = relativePath;
        }

        _unitOfWork.Repository<Facility>().Update(facility);
        await _unitOfWork.Complete();

        return ServiceResult<FacilityDto>.Ok(_mapper.Map<FacilityDto>(facility), "Facility updated successfully.");
    }

    public async Task<ServiceResult<bool>> DeleteFacility(int id)
    {
        var facility = await _unitOfWork.Repository<Facility>().GetByIdAsync(id);

        if (facility == null)
            return ServiceResult<bool>.Fail("Facility not found.");

        _unitOfWork.Repository<Facility>().Remove(facility);
        var result = await _unitOfWork.Complete();

        return result > 0
            ? ServiceResult<bool>.Ok(true, "Facility deleted successfully.")
            : ServiceResult<bool>.Fail("Failed to delete facility.");
    }

    public async Task<ServiceResult<List<FacilityDto>>> GetAllFacilities()
    {
        var facilities = await _unitOfWork.Repository<Facility>()
            .GetAllIncludingAsync(f => f.Address);

        var dtos = _mapper.Map<List<FacilityDto>>(facilities);

        return ServiceResult<List<FacilityDto>>.Ok(dtos);
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
}
