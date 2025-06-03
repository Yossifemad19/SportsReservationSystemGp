using AutoMapper;
using backend.Api.DTOs;
using backend.Api.Errors;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Roles = "Admin")]
public class SportController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SportController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddSport(string SportName)
    {
        if (string.IsNullOrEmpty(SportName))
            return BadRequest("Sport name is required");
        if (SportName.Length > 20)
            return BadRequest("Sport name is too long");

        _unitOfWork.Repository<Sport>().Add(new Sport() { Name = SportName });
        return await _unitOfWork.Complete() > 0 ? Ok("sport added successfully") : BadRequest(new ApiResponse(400, "sport not added"));

    }

    [HttpGet("getAll")]
    public async Task<IActionResult> GetAllSports()
    {
        var sports = await _unitOfWork.Repository<Sport>().GetAllAsync();
        return Ok(_mapper.Map<IReadOnlyList<SportDto>>(sports));
    }

    [HttpPut("Update")]
    public async Task<IActionResult> UpdateSport(SportDto sportDto)
    {

        var existingSport = await _unitOfWork.Repository<Sport>().GetByIdAsync(sportDto.Id);
        if (existingSport == null)
        {
            return NotFound(new ApiResponse(404,"Sport not found"));
        }


        existingSport.Name = sportDto.Name;


        _unitOfWork.Repository<Sport>().Update(existingSport);
        await _unitOfWork.Complete();

        return Ok("Sport updated successfully");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSportById(int id)
    {
        var sport = await _unitOfWork.Repository<Sport>().GetByIdAsync(id);
        if (sport == null)
            return NotFound(new ApiResponse(404, "Sport not found"));

        var sportDto = _mapper.Map<SportDto>(sport);
        return Ok(sportDto);
    }

}

