using backend.Api.DTOs;
using backend.Api.Errors;
using backend.Api.Services;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OwnerAuthController : ControllerBase
{
    private readonly IAuthService _OwnerAuthService;

    public OwnerAuthController(IAuthService OwnerAuthService)
    {
        _OwnerAuthService = OwnerAuthService;
    }


    

    [HttpPost("OwnerRegister")]
    public async Task<IActionResult> OwnerRegister([FromBody] OwnerRegisterDto ownerRegisterDto)
    {
        var result = await _OwnerAuthService.OwnerRegister(ownerRegisterDto, "Owner");

        if (!result.Success)
            return BadRequest(result);

        var owner = new
        {
            
            FullName = ownerRegisterDto.FirstName + " " + ownerRegisterDto.LastName,
            Email = ownerRegisterDto.Email,
            phoneNumber = ownerRegisterDto.PhoneNumber,
            Role = "Owner"
        };

        return Ok(new
        {
            message = "Registered successfully",
            token = result.Data,
            owner = owner
        });
    }

    [HttpPost("OwnerLogin")]
    public async Task<IActionResult> OwnerLogin([FromBody] OwnerLoginDto ownerLoginDto)
    {
        var result = await _OwnerAuthService.OwnerLogin(ownerLoginDto);
        if (!result.Success)
            return BadRequest(result);

        
        if (!bool.TryParse(result.Data.IsApproved, out var isApproved) || !isApproved)
            return Unauthorized(ServiceResult<OwnerResponseDto>.Fail( "Your account is not approved yet. Please wait for admin approval."));

        return Ok(result);
    }

}