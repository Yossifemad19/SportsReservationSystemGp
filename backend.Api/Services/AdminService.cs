using AutoMapper;
using backend.Api.DTOs;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        var owner = await _unitOfWork.Repository<Owner>().FindAsync(x => x.Id == ownerId && x.UserRole == UserRole.Owner);
        if (owner == null) return false;

        owner.IsApproved = true;
        return await _unitOfWork.CompleteAsync() > 0;
    }

    public async Task<bool> RejectOwner(int ownerId)
    {
        var owner = await _unitOfWork.Repository<Owner>().FindAsync(o => o.Id == ownerId);
        if (owner == null) return false;

        _unitOfWork.Repository<Owner>().Remove(owner);
        return await _unitOfWork.CompleteAsync() > 0;
    }


    public async Task<ResponseDto> AdminLogin(AdminLoginDto adminLoginDto)
    {
        var admin = await _unitOfWork.Repository<Admin>().FindAsync(x => x.Email == adminLoginDto.Email);

        if (admin != null && ValidatePassword(adminLoginDto.Password, admin.PasswordHash))
        {

            var token = _tokenService.GenerateToken(admin);

            return new ResponseDto
            {
                Name = admin.FirstName + " " + admin.LastName,
                Email = admin.Email,
                Token = token,
                Role = admin.UserRole.ToString(),
                Message = "Logged in successfully"
            };
        }
        return new ResponseDto { Message = "Invalid email or password" };
    }


    public async Task<List<OwnerRegisterDto>> GetAllOwners()
    {
        var users = await _unitOfWork.Repository<Owner>().GetAllAsync();
        return users.Select(owner => new OwnerRegisterDto
        {
            FirstName = owner.FirstName,
            LastName = owner.LastName,
            Email = owner.Email

        }).ToList();
    }

    public async Task<List<RegisterDto>> GetAllUsers()
    {
        var users = await _unitOfWork.Repository<User>().GetAllAsync();
        return users.Select(u => new RegisterDto
        {
            
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email
            
        }).ToList();
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