
using backend.Api.DTOs;
using backend.Core.Entities;

namespace backend.Api.Services;

public interface IFacilityService
{
    Task<FacilityDto?> GetFacilityById(int id);
    Task<FacilityDto> CreateFacility(FacilityDto FacilityDto,string ownerId);
    // Task<bool> UpdateFacility(FacilityDto FacilityDto);
    Task<bool> DeleteFacility(int id);
}
