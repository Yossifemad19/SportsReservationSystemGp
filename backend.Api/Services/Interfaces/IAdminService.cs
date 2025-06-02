using backend.Api.DTOs;
using backend.Core.Entities;
using Microsoft.AspNetCore.Identity.Data;

namespace backend.Api.Services;

public interface IAdminService
{
    Task<UserResponseDto> AdminLogin(AdminLoginDto adminLoginDto);
    Task<bool> ApproveOwner(int ownerId);
    Task<List<GetAllResponse>> GetAllUsers();
    
    Task<List<GetAllOwnerResponse>> GetAllOwners();
    public Task<GetAllResponse> GetOwnerById(int id);
    public Task<GetAllResponse> GetUserById(int id);
    Task<bool> RejectOwner(int ownerId);
    Task<IEnumerable<UnApprovedOwnerDto>> GetAllUnApprovedOwners();
}

