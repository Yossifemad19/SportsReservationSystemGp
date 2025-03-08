using backend.Api.DTOs;
using backend.Api.Errors;
using backend.Api.Services;
using backend.Core.Entities;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminAuthController:ControllerBase
{
    private readonly IAuthService _authService;

    public AdminAuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.Login(loginDto);
        if(String.IsNullOrEmpty(result))
            return BadRequest(new ApiResponse(400,"Username or password is incorrect"));
        
        return Ok(new 
        {
            message = "Logged in successfully",
            token = result
        });
    }
}