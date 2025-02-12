using backend.Api.DTOs;
using Microsoft.AspNetCore.Identity.Data;

namespace backend.Api.Services;

public interface IAuthService
{
    public string Register(RegisterDto registerDto);
    public string Login(LoginDto loginDto);
}