using backend.Api.DTOs;
using backend.Core.Entities;
using Microsoft.AspNetCore.Identity.Data;

namespace backend.Api.Services;

public interface IAuthService
{
    public Task<string> Register(RegisterDto registerDto,UserRole userRole);
    public Task<ResponseDto> Login(LoginDto loginDto);
    public Task<string> OwnerRegister(OwnerRegisterDto ownerRegisterDto, UserRole userRole);
    public Task<ResponseDto> OwnerLogin(OwnerLoginDto ownerLoginDto);
    
    
}