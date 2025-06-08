using backend.Api.DTOs;
using backend.Core.Entities;
using Microsoft.AspNetCore.Identity.Data;
using System.Threading.Tasks;

namespace backend.Api.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<string>> Register(RegisterDto registerDto, string roleName);
        Task<ServiceResult<UserResponseDto>> Login(LoginDto loginDto);
        Task<ServiceResult<string>> OwnerRegister(OwnerRegisterDto ownerRegisterDto, string roleName);
        Task<ServiceResult<OwnerResponseDto>> OwnerLogin(OwnerLoginDto ownerLoginDto);

        Task<ServiceResult<GetAllResponse>> GetUserById(int id);
        Task<ServiceResult<string>> ForgotPassword(string email);
        Task<ServiceResult<string>> ResetPassword(PasswordDto passwordDto);
        Task<ServiceResult<UserProfileDto>> UpdateUserProfile(int userId, UserProfileDto userProfile);
        Task<ServiceResult<bool>> DeleteUser(int userId);

        Task<bool> IsEmailExist(string email);
    }
}