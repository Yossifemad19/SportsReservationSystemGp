using System.Security.Claims;
using AutoMapper;
using backend.Api.DTOs;
using backend.Api.Errors;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourtController:ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CourtController(IUnitOfWork unitOfWork,IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    //[Authorize(Roles = "Owner")]
    [HttpPost("add")]
    public async Task<IActionResult> AddCourt(CourtDto courtDto)
    {
        
        var court = _mapper.Map<Court>(courtDto);
        
        _unitOfWork.Repository<Court>().Add(court);
        return await _unitOfWork.Complete()>0?Ok("court add"):
            BadRequest(new ApiResponse(400,"court add failed"));
    }


    [HttpGet("getAll")]
    public async Task<ActionResult<IReadOnlyList<CourtDto>>> GetAllCourts([FromQuery] CourtSpecParams specParams)
    {
        var spec = new CourtWithFiltersSpecification(specParams);
        var courts = await _unitOfWork.Repository<Court>().GetAllWithSpecAsync(spec);
        return Ok(_mapper.Map<IReadOnlyList<CourtDto>>(courts));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCourt(CourtDto courtDto)
    {

        var existingCourt = await _unitOfWork.Repository<Court>().GetByIdAsync(courtDto.Id);
        if (existingCourt == null)
        {
            return NotFound(new ApiResponse(404,"Court not found"));
        }


        existingCourt.Name = courtDto.Name;
        existingCourt.Capacity = courtDto.Capacity;
        existingCourt.PricePerHour = courtDto.PricePerHour;
        existingCourt.FacilityId = courtDto.FacilityId;
        existingCourt.SportId = courtDto.SportId;



        _unitOfWork.Repository<Court>().Update(existingCourt);
        await _unitOfWork.Complete();

        return Ok("Court updated successfully");
    }

    [HttpGet("GetById")]
    public async Task<IActionResult> GetCourtById(int id)
    {
        var court = await _unitOfWork.Repository<Court>().GetByIdAsync(id);
        if (court == null)
        {
            return NotFound(new ApiResponse(404, "Court not found"));
        }

        return Ok(court);
    }

}

    
 

    