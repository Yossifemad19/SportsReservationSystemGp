using System.Threading.Tasks;
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
        if(!result.Success)
            return BadRequest(result.Message);
        
        return Ok(result);
    }

    [HttpPost("approve/{ownerId}")]
    public async Task<IActionResult> ApproveOwner(int ownerId)
    {
        var result = await _adminService.ApproveOwner(ownerId);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _adminService.GetAllUsers();
        if(!result.Success)
            return BadRequest(result.Message);
        return Ok(result);
    }


    [HttpGet("owners")]
    public async Task<IActionResult> GetAllOwners()
    {
        var result = await _adminService.GetAllOwners();
        if(!result.Success)
            return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpPost("rejectOwner/{ownerId}")]
    public async Task<IActionResult> RejectOwner(int ownerId)
    {
        var result = await _adminService.RejectOwner(ownerId);
        if (!result.Success) return BadRequest(result.Message);

        return Ok(result);
    }
    [HttpGet("unapprovedOwners")]
    public async Task<IActionResult> GetUnapprovedOwners()
    {
        var result = await _adminService.GetAllUnApprovedOwners();
        if(!result.Success) return BadRequest(result.Message);
        return Ok(result);
    }


    [HttpGet("GetOwnerById")]
    public async Task<IActionResult> GetOwnerById(int id)
    {
        var result = await _adminService.GetOwnerById(id);
        if(!result.Success) return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpGet("GetUserById")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var result = await _adminService.GetUserById(id);
        if(!result.Success) return BadRequest(result.Message);
        return Ok(result);
    }

}

