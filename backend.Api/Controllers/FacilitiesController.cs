using System.Security.Claims;
using backend.Api.DTOs;
using backend.Api.Errors;
using backend.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class FacilitiesController : ControllerBase
{
    private readonly IFacilityService _facilityService;
    private readonly IWebHostEnvironment _env;

    public FacilitiesController(IFacilityService facilityService, IWebHostEnvironment env)
    {
        _facilityService = facilityService;
        _env = env;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _facilityService.GetFacilityById(id);

        if (!result.Success || result.Data == null)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Create([FromForm] FacilityDto facilityDto)
    {
        var ownerId = User.FindFirst("sub")?.Value;

        var result = await _facilityService.CreateFacility(facilityDto, ownerId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> UpdateFacility([FromForm] FacilityDto facilityDto)
    {
        var ownerId = User.FindFirst("sub")?.Value;

        if (facilityDto == null || facilityDto.Id <= 0)
            return BadRequest(ServiceResult<FacilityDto>.Fail("Facility data or ID is missing."));

        var result = await _facilityService.UpdateFacility(facilityDto, ownerId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _facilityService.DeleteFacility(id);

        if (!result.Success || result.Data == false)
            return NotFound(result);

        return Ok(result);
    }

    //[HttpGet("GetAll")]
    //public async Task<IActionResult> GetAllFacilities()
    //{
    //    var result = await _facilityService.GetAllFacilities();

    //    if (!result.Success || result.Data == null || !result.Data.Any())
    //        return NotFound(result.Message);

    //    return Ok(result);
    //}
    [HttpGet]
    public async Task<IActionResult> GetAllFacilities([FromQuery] bool isOwner = false,int? sportId = null)
    {
        var ownerId = User.FindFirst("sub")?.Value;

        var result = await _facilityService.GetAllFacilities(isOwner, ownerId, sportId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
