using backend.Api.DTOs;
using backend.Api.Services;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[Route("api/[controller]")]
public class OwnerAuthController : ControllerBase
{
    private readonly IAuthService _OwnerAuthService;

    public OwnerAuthController(IAuthService OwnerAuthService)
    {
        _OwnerAuthService = OwnerAuthService;
    }


    [HttpPost("OwnerRegister")]
    public async Task<IActionResult> OwnerRegister([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _OwnerAuthService.Register(registerDto,UserRole.Owner);

        if (String.IsNullOrEmpty(result))
            return BadRequest("Failed to register user");

        return Ok(result);
    }

    [HttpPost("OwnerLogin")]
    public async Task<IActionResult> OwnerLogin([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var result = await _OwnerAuthService.Login(loginDto);
        if (String.IsNullOrEmpty(result))
            return BadRequest("Username or password is incorrect");

        return Ok(result);
    }
}
