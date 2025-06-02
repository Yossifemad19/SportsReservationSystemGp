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


    public async Task<bool> ApproveOwner(int ownerId)
    {
        // Get the Owner role
        var ownerRoleSpec = new UserRoleSpecification("Owner");
        var ownerRole = await _unitOfWork.Repository<UserRole>().GetFirstOrDefaultAsync(ownerRoleSpec);
        
        // Find the owner and ensure they have the Owner role
        var owner = await _unitOfWork.Repository<Owner>().FindAsync(x => x.Id == ownerId && x.UserRoleId == ownerRole.Id);
        if (owner == null) return false;

        owner.IsApproved = true;
        return await _unitOfWork.Complete() > 0;
    }

    public async Task<bool> RejectOwner(int ownerId)
    {
        var owner = await _unitOfWork.Repository<Owner>().FindAsync(o => o.Id == ownerId);
        if (owner == null) return false;

        _unitOfWork.Repository<Owner>().Remove(owner);
        return await _unitOfWork.Complete() > 0;
    }


    public async Task<UserResponseDto> AdminLogin(AdminLoginDto adminLoginDto)
    {
        var admin = await _unitOfWork.Repository<Admin>().FindAsync(x => x.Email == adminLoginDto.Email);

        if (admin != null && ValidatePassword(adminLoginDto.Password, admin.PasswordHash))
        {
            // Get the Admin role
            var adminRole = await _unitOfWork.Repository<UserRole>().GetByIdAsync(admin.UserRoleId);
            
            var token = _tokenService.GenerateToken(admin);

            return new UserResponseDto
            {
                Name = admin.FirstName + " " + admin.LastName,
                Email = admin.Email,
                Token = token,
                Role = adminRole?.RoleName ?? "Unknown",
                Message = "Logged in successfully"
            };
        }
        return null;
    }


    public async Task<List<GetAllOwnerResponse>> GetAllOwners()
    {
        var owners = await _unitOfWork.Repository<Owner>().GetAllAsync();
        return owners.Select(owner => new GetAllOwnerResponse
        {
            Id = owner.Id,
            FirstName = owner.FirstName,
            LastName = owner.LastName,
            Email = owner.Email,
            PhoneNumber = owner.PhoneNumber,
            IsApproved = owner.IsApproved
        }).ToList();
    }

    public async Task<List<GetAllResponse>> GetAllUsers()
    {
        var users = await _unitOfWork.Repository<User>().GetAllAsync();
        return users.Select(user => new GetAllResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
        }).ToList();
    }

    public async Task<IEnumerable<UnApprovedOwnerDto>> GetAllUnApprovedOwners()
    {
        var unapprovedOwners = await _unitOfWork.Repository<Owner>().GetAllAsync();

        return unapprovedOwners.Where(o => !o.IsApproved) 
            .Select(o => new UnApprovedOwnerDto
            {
                FirstName = o.FirstName,
                LastName = o.LastName,  
                Email = o.Email,
                IsApproved = o.IsApproved
            });
    }


    public async Task<GetAllResponse> GetOwnerById(int id)
    {
        var owner = await _unitOfWork.Repository<Owner>()
                                        .GetByIdAsync(id);

        if (owner == null)
        {
            throw new Exception($"Owner with id {id} not found.");
        }


        return new GetAllResponse
        {
            Id = owner.Id,
            FirstName = owner.FirstName,
            LastName = owner.LastName,
            Email = owner.Email,
            PhoneNumber = owner.PhoneNumber,


        };
    }

    public async Task<GetAllResponse> GetUserById(int id)
    {
        var user = await _unitOfWork.Repository<User>()
                                        .GetByIdAsync(id);

        if (user == null)
        {
            throw new Exception($"User with id {id} not found.");
        }


        return new GetAllResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,


        };
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