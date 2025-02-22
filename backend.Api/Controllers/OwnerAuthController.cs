using backend.Api.DTOs;
using backend.Api.Services;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[Route("api/[controller]")]
public class OwnerAuthController : ControllerBase
{
    private readonly IGenericRepository<UserCredential> _repository;
    private readonly IAuthService _OwnerAuthService;

    public OwnerAuthController(IGenericRepository<UserCredential> repository, IAuthService OwnerAuthService)
    {
        _repository = repository;
        _OwnerAuthService = OwnerAuthService;
    }


    [HttpPost("OwnerRegister")]
    public async Task<IActionResult> OwnerRegister([FromBody] FacilityOwnerDTO facilityOwnerDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var result = await _OwnerAuthService.OwnerRegister(facilityOwnerDTO);

        if (String.IsNullOrEmpty(result))
            return BadRequest("Failed to register user");

        return Ok(result);
    }
}
