using backend.Api.DTOs;
using backend.Core.Entities;
using Microsoft.AspNetCore.Identity.Data;

namespace backend.Api.Services;

public interface IAdminService
{
    Task<ResponseDto> AdminLogin(AdminLoginDto adminLoginDto);
    Task<bool> ApproveOwner(int ownerId);
    Task<List<RegisterDto>> GetAllUsers();
    
    Task<List<OwnerRegisterDto>> GetAllOwners();
    
    Task<bool> RejectOwner(int ownerId);
    Task<IEnumerable<UnApprovedOwnerDto>> GetAllUnApprovedOwners();
}

