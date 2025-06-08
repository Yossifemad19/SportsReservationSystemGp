using backend.Api.DTOs;
using backend.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Api.Services
{
    public interface IAdminService
    {
        Task<ServiceResult<UserResponseDto>> AdminLogin(AdminLoginDto adminLoginDto);
        Task<ServiceResult<bool>> ApproveOwner(int ownerId);
        Task<ServiceResult<List<GetAllResponse>>> GetAllUsers();
        Task<ServiceResult<List<GetAllOwnerResponse>>> GetAllOwners();
        Task<ServiceResult<GetAllResponse>> GetOwnerById(int id);
        Task<ServiceResult<GetAllResponse>> GetUserById(int id);
        Task<ServiceResult<bool>> RejectOwner(int ownerId);
        Task<ServiceResult<IEnumerable<UnApprovedOwnerDto>>> GetAllUnApprovedOwners();
    }
}