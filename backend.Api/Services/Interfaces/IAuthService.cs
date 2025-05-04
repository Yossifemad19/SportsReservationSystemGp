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
    
    
}