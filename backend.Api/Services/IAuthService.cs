using backend.Api.DTOs;
using Microsoft.AspNetCore.Identity.Data;

namespace backend.Api.Services;

public interface IAuthService
{
    public Task<string> Register(RegisterDto registerDto);
    public Task<string> Login(LoginDto loginDto);
    public Task<string> OwnerRegister(FacilityOwnerDTO facilityOwnerDTO);
    public Task<string> OwnerLogin(OwnerLoginDto ownerLoginDto);

}