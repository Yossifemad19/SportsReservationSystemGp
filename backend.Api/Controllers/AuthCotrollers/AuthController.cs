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

    [HttpPut("UserProfile")]
    public async Task<IActionResult> UpdateUserProfile( [FromBody] UserProfileDto userProfile)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
        {
            return BadRequest(new ApiResponse(400, "Invalid or missing user ID in token."));
        }

        if (userProfile == null || userId <= 0)
        {
            return BadRequest(new ApiResponse(400, "Invalid user profile data or user ID."));
        }

        try
        {
            var result = await _authService.UpdateUserProfile(userId, userProfile);

            if (result == null)
            {
                
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update the user profile.");
            }

            return Ok(result); 
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = ex.Message }); 
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message }); 
        }
    }

    //[HttpDelete("{userId}")]
    //public async Task<IActionResult> DeleteUser(int userId)
    //{
    //    try
    //    {
    //        var result = await _authService.DeleteUser(userId);

    //        if (result)
    //        {
    //            return NoContent(); 
    //        }

    //        return StatusCode(StatusCodes.Status500InternalServerError,
    //            new { message = "Failed to delete the user." });
    //    }
    //    catch (Exception ex)
    //    {
    //        if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
    //        {
    //            return NotFound(new { message = ex.Message }); 
    //        }

    //        return StatusCode(StatusCodes.Status500InternalServerError,
    //            new { message = ex.Message }); // 500 Internal Server Error
    //    }
    //}



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