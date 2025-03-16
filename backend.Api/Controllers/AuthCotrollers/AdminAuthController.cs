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
    private readonly IAdminService _adminService;

    public AdminAuthController(IAdminService adminService)
    {
        
        _adminService = adminService;
    }

    [HttpPost("login")]
    public async Task<ActionResult> AdminLogin([FromBody] AdminLoginDto adminLoginDto)
    {
        var result = await _adminService.AdminLogin(adminLoginDto);
        if(result == null)
            return BadRequest(new ApiResponse(400,"Username or password is incorrect"));
        
        return Ok(new 
        {
            Data = result
        });
    }

    [HttpPost("approve/{ownerId}")]
    public async Task<IActionResult> ApproveOwner(int ownerId)
    {
        var result = await _adminService.ApproveOwner(ownerId);
        if (!result)
            return BadRequest(new
            {
                message = "Failed to approve owner or owner not found."
            });

        return Ok(new
        {
            message = "Owner approved successfully."
        });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsers();
        return Ok(users);
    }


    [HttpGet("owners")]
    public async Task<IActionResult> GetAllOwners()
    {
        var owners = await _adminService.GetAllOwners();
        return Ok(owners);
    }

    [HttpPost("rejectOwner/{ownerId}")]
    public async Task<IActionResult> RejectOwner(int ownerId)
    {
        var result = await _adminService.RejectOwner(ownerId);
        if (!result) return BadRequest(new { message = "Failed to reject owner" });

        return Ok(new { message = "Owner rejected successfully" });
    }

}
