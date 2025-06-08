using backend.Api.DTOs;
using backend.Core.Entities;

namespace backend.Api.Services;

public interface IFacilityService
{
    Task<ServiceResult<FacilityDto>> GetFacilityById(int id);
    Task<ServiceResult<FacilityDto>> CreateFacility(FacilityDto facilityDto, string ownerId);
    Task<ServiceResult<FacilityDto>> UpdateFacility(FacilityDto facilityDto, string ownerId);
    Task<ServiceResult<bool>> DeleteFacility(int id);
    Task<ServiceResult<List<FacilityDto>>> GetAllFacilities();
}