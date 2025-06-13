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
    private readonly IWebHostEnvironment _env;

    public SportController(IUnitOfWork unitOfWork, IMapper mapper , IWebHostEnvironment env)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _env = env;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddSport([FromForm] SportDto sportDto)
    {
        if (string.IsNullOrEmpty(sportDto.Name))
            return BadRequest("Sport name is required");

        if (sportDto.Name.Length > 20)
            return BadRequest("Sport name is too long");

        string? relativePath = null;

        if (sportDto.Image != null)
        {
            
            var imagesFolder = Path.Combine(_env.WebRootPath, "images", "Sports");
            if (!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);

            
            var imageFile = $"{Guid.NewGuid()}-{sportDto.Image.FileName}";

            
            var filePath = Path.Combine(imagesFolder, imageFile);

            
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await sportDto.Image.CopyToAsync(fileStream);
            }

            
            relativePath = $"images/Sports/{imageFile}";
        }

        var sport = new Sport
        {
            Name = sportDto.Name,
            ImageUrl = relativePath
        };

        _unitOfWork.Repository<Sport>().Add(sport);
        var result = await _unitOfWork.Complete();

        if (result <= 0)
            return BadRequest(new ApiResponse(400, "Sport not added"));

        return Ok("Sport added successfully");
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
            return NotFound(new ApiResponse(404, "Sport not found"));
        }

        string? relativePath = null;

        if (sportDto.Image != null)
        {
            var imagesFolder = Path.Combine(_env.WebRootPath, "images", "Sports");
            if (!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);

            var imageFile = $"{Guid.NewGuid()}-{sportDto.Image.FileName}";
            var filePath = Path.Combine(imagesFolder, imageFile);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await sportDto.Image.CopyToAsync(fileStream);
            }

            relativePath = $"images/Sports/{imageFile}";
        }

        existingSport.Name = sportDto.Name;

        if (relativePath != null)
        {
            existingSport.ImageUrl = relativePath;
        }

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

