using backend.Api.DTOs;
using backend.Core.Entities;
using Microsoft.AspNetCore.Identity.Data;

namespace backend.Api.Services;

public interface IAuthService
{
    public Task<string?> Register(RegisterDto registerDto,UserRole userRole);
    public Task<UserResponseDto?> Login(LoginDto loginDto);
    public Task<string?> OwnerRegister(OwnerRegisterDto ownerRegisterDto, UserRole userRole);
    public Task<OwnerResponseDto?> OwnerLogin(OwnerLoginDto ownerLoginDto);
    public Task<GetAllResponse?> GetUserById(int id);

    public Task<string?> ForgotPassword(string email);
    public Task<string?> ResetPassword(PasswordDto passwordDto);


}