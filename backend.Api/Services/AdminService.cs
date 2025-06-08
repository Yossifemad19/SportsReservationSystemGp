using AutoMapper;
using backend.Api.DTOs;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace backend.Api.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public AdminService(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult<bool>> ApproveOwner(int ownerId)
    {
        var ownerRoleSpec = new UserRoleSpecification("Owner");
        var ownerRole = await _unitOfWork.Repository<UserRole>().GetFirstOrDefaultAsync(ownerRoleSpec);

        if (ownerRole == null)
            return ServiceResult<bool>.Fail("Owner role not found.");

        var owner = await _unitOfWork.Repository<Owner>().FindAsync(x => x.Id == ownerId && x.UserRoleId == ownerRole.Id);
        if (owner == null)
            return ServiceResult<bool>.Fail("Owner not found.");

        owner.IsApproved = true;
        bool result = await _unitOfWork.Complete() > 0;

        return result
            ? ServiceResult<bool>.Ok(true, "Owner approved.")
            : ServiceResult<bool>.Fail("Failed to approve owner.");
    }

    public async Task<ServiceResult<bool>> RejectOwner(int ownerId)
    {
        var owner = await _unitOfWork.Repository<Owner>().FindAsync(o => o.Id == ownerId);
        if (owner == null)
            return ServiceResult<bool>.Fail("Owner not found.");

        _unitOfWork.Repository<Owner>().Remove(owner);
        bool result = await _unitOfWork.Complete() > 0;

        return result
            ? ServiceResult<bool>.Ok(true, "Owner rejected and deleted.")
            : ServiceResult<bool>.Fail("Failed to reject owner.");
    }

    public async Task<ServiceResult<UserResponseDto>> AdminLogin(AdminLoginDto adminLoginDto)
    {
        var admin = await _unitOfWork.Repository<Admin>().FindAsync(x => x.Email == adminLoginDto.Email);
        if (admin == null || !ValidatePassword(adminLoginDto.Password, admin.PasswordHash))
            return ServiceResult<UserResponseDto>.Fail("Invalid email or password.");

        var adminRole = await _unitOfWork.Repository<UserRole>().GetByIdAsync(admin.UserRoleId);
        var token = _tokenService.GenerateToken(admin);

        var response = new UserResponseDto
        {
            Id = admin.Id,
            Name = $"{admin.FirstName} {admin.LastName}",
            Email = admin.Email,
            Token = token,
            Role = adminRole?.RoleName ?? "Unknown",
            Message = "Logged in successfully"
        };

        return ServiceResult<UserResponseDto>.Ok(response);
    }

    public async Task<ServiceResult<List<GetAllOwnerResponse>>> GetAllOwners()
    {
        var owners = await _unitOfWork.Repository<Owner>().GetAllAsync();

        var response = owners.Select(owner => new GetAllOwnerResponse
        {
            Id = owner.Id,
            FirstName = owner.FirstName,
            LastName = owner.LastName,
            Email = owner.Email,
            PhoneNumber = owner.PhoneNumber,
            IsApproved = owner.IsApproved
        }).ToList();

        return ServiceResult<List<GetAllOwnerResponse>>.Ok(response);
    }

    public async Task<ServiceResult<List<GetAllResponse>>> GetAllUsers()
    {
        var users = await _unitOfWork.Repository<User>().GetAllAsync();

        var response = users.Select(user => new GetAllResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        }).ToList();

        return ServiceResult<List<GetAllResponse>>.Ok(response);
    }

    public async Task<ServiceResult<IEnumerable<UnApprovedOwnerDto>>> GetAllUnApprovedOwners()
    {
        var unapprovedOwners = await _unitOfWork.Repository<Owner>().GetAllAsync();

        var response = unapprovedOwners
            .Where(o => !o.IsApproved)
            .Select(o => new UnApprovedOwnerDto
            {
                FirstName = o.FirstName,
                LastName = o.LastName,
                Email = o.Email,
                IsApproved = o.IsApproved
            });

        return ServiceResult<IEnumerable<UnApprovedOwnerDto>>.Ok(response.ToList());
    }

    public async Task<ServiceResult<GetAllResponse>> GetOwnerById(int id)
    {
        var owner = await _unitOfWork.Repository<Owner>().GetByIdAsync(id);
        if (owner == null)
            return ServiceResult<GetAllResponse>.Fail($"Owner with id {id} not found.");

        var response = new GetAllResponse
        {
            Id = owner.Id,
            FirstName = owner.FirstName,
            LastName = owner.LastName,
            Email = owner.Email,
            PhoneNumber = owner.PhoneNumber
        };

        return ServiceResult<GetAllResponse>.Ok(response);
    }

    public async Task<ServiceResult<GetAllResponse>> GetUserById(int id)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
        if (user == null)
            return ServiceResult<GetAllResponse>.Fail($"User with id {id} not found.");

        var response = new GetAllResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };

        return ServiceResult<GetAllResponse>.Ok(response);
    }

    public static string GetHashedPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool ValidatePassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
