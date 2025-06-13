using System.Security.Claims;
using AutoMapper;
using backend.Api.Controllers;
using backend.Api.DTOs;
using backend.Api.Errors;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace backend.Tests.Controllers;

public class SportControllerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
    private readonly Mock<IGenericRepository<Sport>> _mockSportRepository;
    private readonly SportController _controller;

    public SportControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        _mockSportRepository = new Mock<IGenericRepository<Sport>>();
        
        _mockUnitOfWork.Setup(uow => uow.Repository<Sport>())
            .Returns(_mockSportRepository.Object);
        
        _controller = new SportController(_mockUnitOfWork.Object, _mockMapper.Object, _mockWebHostEnvironment.Object);
    }

    [Fact]
    public async Task AddSport_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var sportDto = new SportDto
        {
            Name = "Football",
            Image = new FormFile(Stream.Null, 0, 0, "Data", "test.jpg")
        };

        _mockWebHostEnvironment.Setup(env => env.WebRootPath)
            .Returns("wwwroot");

        _mockUnitOfWork.Setup(uow => uow.Complete())
            .ReturnsAsync(1);

        // Act
        var result = await _controller.AddSport(sportDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Sport added successfully", okResult.Value);
        _mockSportRepository.Verify(repo => repo.Add(It.IsAny<Sport>()), Times.Once);
    }

    [Fact]
    public async Task AddSport_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var sportDto = new SportDto
        {
            Name = "",
            Image = new FormFile(Stream.Null, 0, 0, "Data", "test.jpg")
        };

        // Act
        var result = await _controller.AddSport(sportDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Sport name is required", badRequestResult.Value);
    }

    [Fact]
    public async Task AddSport_WithLongName_ShouldReturnBadRequest()
    {
        // Arrange
        var sportDto = new SportDto
        {
            Name = new string('a', 21), // Name longer than 20 characters
            Image = new FormFile(Stream.Null, 0, 0, "Data", "test.jpg")
        };

        // Act
        var result = await _controller.AddSport(sportDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Sport name is too long", badRequestResult.Value);
    }

    [Fact]
    public async Task AddSport_WhenSaveFails_ShouldReturnBadRequest()
    {
        // Arrange
        var sportDto = new SportDto
        {
            Name = "Football",
            Image = new FormFile(Stream.Null, 0, 0, "Data", "test.jpg")
        };

        _mockWebHostEnvironment.Setup(env => env.WebRootPath)
            .Returns("wwwroot");

        _mockUnitOfWork.Setup(uow => uow.Complete())
            .ReturnsAsync(0);

        // Act
        var result = await _controller.AddSport(sportDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
        Assert.Equal(400, apiResponse.StatusCode);
        Assert.Equal("Sport not added", apiResponse.Messege);
    }

    [Fact]
    public async Task GetAllSports_ShouldReturnSports()
    {
        // Arrange
        var sports = new List<Sport>
        {
            new() { Id = 1, Name = "Football", ImageUrl = "football.jpg" },
            new() { Id = 2, Name = "Basketball", ImageUrl = "basketball.jpg" }
        };

        var sportDtos = new List<SportDto>
        {
            new() { Id = 1, Name = "Football", ImageUrl = "football.jpg" },
            new() { Id = 2, Name = "Basketball", ImageUrl = "basketball.jpg" }
        };

        _mockSportRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(sports);

        _mockMapper.Setup(mapper => mapper.Map<IReadOnlyList<SportDto>>(sports))
            .Returns(sportDtos);

        // Act
        var result = await _controller.GetAllSports();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<SportDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public async Task UpdateSport_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var sportDto = new SportDto { Id = 1, Name = "Updated Football" };
        var existingSport = new Sport { Id = 1, Name = "Football" };

        _mockSportRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(existingSport);

        _mockUnitOfWork.Setup(uow => uow.Complete())
            .ReturnsAsync(1);

        // Act
        var result = await _controller.UpdateSport(sportDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Sport updated successfully", okResult.Value);
        _mockSportRepository.Verify(repo => repo.Update(It.IsAny<Sport>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSport_WhenSportNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var sportDto = new SportDto { Id = 999, Name = "Updated Football" };

        _mockSportRepository.Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((Sport?)null);

        // Act
        var result = await _controller.UpdateSport(sportDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(notFoundResult.Value);
        Assert.Equal(404, apiResponse.StatusCode);
        Assert.Equal("Sport not found", apiResponse.Messege);
    }

    [Fact]
    public async Task GetSportById_WhenSportExists_ShouldReturnSport()
    {
        // Arrange
        var sport = new Sport { Id = 1, Name = "Football", ImageUrl = "football.jpg" };
        var sportDto = new SportDto { Id = 1, Name = "Football", ImageUrl = "football.jpg" };

        _mockSportRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(sport);

        _mockMapper.Setup(mapper => mapper.Map<SportDto>(sport))
            .Returns(sportDto);

        // Act
        var result = await _controller.GetSportById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<SportDto>(okResult.Value);
        Assert.Equal(1, returnValue.Id);
        Assert.Equal("Football", returnValue.Name);
    }

    [Fact]
    public async Task GetSportById_WhenSportDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        _mockSportRepository.Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((Sport?)null);

        // Act
        var result = await _controller.GetSportById(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(notFoundResult.Value);
        Assert.Equal(404, apiResponse.StatusCode);
        Assert.Equal("Sport not found", apiResponse.Messege);
    }
} 