using backend.Api.DTOs;
using backend.Api.Errors;
using backend.Api.Services;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController: ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
       
        var result = await _authService.Register(registerDto, "Customer");
        
        if(String.IsNullOrEmpty(result))
            return BadRequest(new ApiResponse(400,"Failed to register user"));


        return Ok(new
        {
            message = "Registered successfully",
            FullName = registerDto.FirstName + " " + registerDto.LastName,
            Email = registerDto.Email,
            phoneNumber = registerDto.PhoneNumber,
            Role = "Customer",
            token = result
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.Login(loginDto);
        if (result == null)
            return BadRequest(new ApiResponse(400, "Username or password is incorrect"));

        return Ok(new 
        {
            Data = result
        });
    }

    [HttpGet("UserProfile")]
    public async Task<IActionResult> GetUserById()
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value);
        var user = await _authService.GetUserById(userId);
        if (user == null)
            return NotFound(new ApiResponse(404, "User not found"));

        return Ok(user);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {

        var result = await _authService.ForgotPassword(forgotPasswordDto.Email);
        if (string.IsNullOrEmpty(result))
            return BadRequest(new ApiResponse(400, "Failed to send reset password link"));

        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordDto passwordDto)
    {
        var result = await _authService.ResetPassword(passwordDto);
        if (string.IsNullOrEmpty(result))
            return BadRequest(new ApiResponse(400, "Failed to reset password"));

        return Ok(result);
    }

}