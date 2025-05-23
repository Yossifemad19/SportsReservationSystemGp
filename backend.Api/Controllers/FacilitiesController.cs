﻿using System.Security.Claims;
using backend.Api.DTOs;
using backend.Api.Errors;
using backend.Api.Services;
using backend.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class FacilitiesController : ControllerBase
{
    private readonly IFacilityService _facilityService;
    private readonly IWebHostEnvironment _env;

    public FacilitiesController(IFacilityService facilityService,IWebHostEnvironment env)
    {
        _facilityService = facilityService;
        _env = env;
    }



    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var facility = await _facilityService.GetFacilityById(id);
        if (facility == null) return NotFound(new ApiResponse(404, "facility not found"));
        return Ok(facility);
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Create([FromForm] FacilityDto facilityDto)
    {

        
        
        var ownerId = User.FindFirst("sub")?.Value;

        var createdFacility = await _facilityService.CreateFacility(facilityDto, ownerId);

        return createdFacility != null ? (IActionResult)Ok(createdFacility) :
            BadRequest(new ApiResponse(400, "facility could not be created"));

        // return CreatedAtAction(nameof(Get), new { id = createdFacility.Id }, createdFacility);
    }

    // [HttpPut("{id}")]
    // public async Task<IActionResult> Update(int id, [FromBody] FacilityDto facilityDto)
    // {
    //     if (id != facilityDto.Id) 
    //         return BadRequest();
    //
    //     var ExistingFacility = await _facilityService.UpdateFacility(facilityDto);
    //     if (!ExistingFacility) return NotFound();
    //
    //     return BadRequest();
    // }

    [HttpDelete("{id}")]
    //[Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id)
    {
        var ExistingFacility = await _facilityService.DeleteFacility(id);

        if (!ExistingFacility) return NotFound(new ApiResponse(404, "facility could not be found"));

        return Ok("facility deleted");
    }

    [HttpGet("GetAll")]
    //[Authorize(Roles = "Owner")]
    public async Task<IActionResult> GetAllFacilities()
    {
        var facilities = await _facilityService.GetAllFacilities();
        if (facilities == null)
        {
            return NotFound(new ApiResponse(404, "No facilities found"));
        }

        return Ok(facilities);
    }

}
