using backend.Api.DTOs;
using backend.Core.Entities;
using Microsoft.AspNetCore.Identity.Data;

namespace backend.Api.Services;

public interface IAuthService
{
    public Task<string> Register(RegisterDto registerDto,UserRole userRole);
    public Task<string> Login(LoginDto loginDto);
  

}