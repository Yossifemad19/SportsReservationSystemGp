using backend.Api.DTOs;
using backend.Core.Entities;
using Microsoft.AspNetCore.Identity.Data;

namespace backend.Api.Services;

public interface IAuthService
{
    public Task<string> Register(RegisterDto registerDto, string roleName);
    public Task<UserResponseDto> Login(LoginDto loginDto);
    public Task<string> OwnerRegister(OwnerRegisterDto ownerRegisterDto, string roleName);
    public Task<OwnerResponseDto> OwnerLogin(OwnerLoginDto ownerLoginDto);
    
    public Task<GetAllResponse> GetUserById(int id);

    public Task<string> ForgotPassword(string email);
    public Task<string> ResetPassword(PasswordDto passwordDto);
    public Task<UserProfileDto> UpdateUserProfile(int userId, UserProfileDto userProfile);
    public  Task<bool> DeleteUser(int userId);


}