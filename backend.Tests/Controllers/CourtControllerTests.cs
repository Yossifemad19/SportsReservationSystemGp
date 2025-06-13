using System.Security.Claims;
using AutoMapper;
using backend.Api.Controllers;
using backend.Api.DTOs;
using backend.Api.Errors;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace backend.Tests.Controllers;

public class CourtControllerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CourtController _controller;

    public CourtControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _controller = new CourtController(_mockUnitOfWork.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task AddCourt_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var courtDto = new CourtDto
        {
            Name = "Tennis Court 1",
            Capacity = 4,
            PricePerHour = 100,
            FacilityId = 1,
            SportId = 1
        };

        var court = new Court
        {
            Name = courtDto.Name,
            Capacity = courtDto.Capacity,
            PricePerHour = courtDto.PricePerHour,
            FacilityId = courtDto.FacilityId,
            SportId = courtDto.SportId
        };

        _mockMapper.Setup(m => m.Map<Court>(courtDto))
            .Returns(court);

        var mockRepo = new Mock<IGenericRepository<Court>>();
        _mockUnitOfWork.Setup(uow => uow.Repository<Court>())
            .Returns(mockRepo.Object);
        _mockUnitOfWork.Setup(uow => uow.Complete())
            .ReturnsAsync(1);

        // Act
        var result = await _controller.AddCourt(courtDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("court add", okResult.Value);
        mockRepo.Verify(r => r.Add(court), Times.Once);
    }

    [Fact]
    public async Task AddCourt_WhenSaveFails_ShouldReturnBadRequest()
    {
        // Arrange
        var courtDto = new CourtDto
        {
            Name = "Tennis Court 1",
            Capacity = 4,
            PricePerHour = 100,
            FacilityId = 1,
            SportId = 1
        };

        var court = new Court
        {
            Name = courtDto.Name,
            Capacity = courtDto.Capacity,
            PricePerHour = courtDto.PricePerHour,
            FacilityId = courtDto.FacilityId,
            SportId = courtDto.SportId
        };

        _mockMapper.Setup(m => m.Map<Court>(courtDto))
            .Returns(court);

        var mockRepo = new Mock<IGenericRepository<Court>>();
        _mockUnitOfWork.Setup(uow => uow.Repository<Court>())
            .Returns(mockRepo.Object);
        _mockUnitOfWork.Setup(uow => uow.Complete())
            .ReturnsAsync(0);

        // Act
        var result = await _controller.AddCourt(courtDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
        Assert.Equal(400, apiResponse.StatusCode);
        Assert.Equal("court add failed", apiResponse.Messege);
    }

    [Fact]
    public async Task GetAllCourts_ShouldReturnCourts()
    {
        // Arrange
        var specParams = new CourtSpecParams();
        var courts = new List<Court>
        {
            new()
            {
                Id = 1,
                Name = "Tennis Court 1",
                Capacity = 4,
                PricePerHour = 100,
                FacilityId = 1,
                SportId = 1
            },
            new()
            {
                Id = 2,
                Name = "Tennis Court 2",
                Capacity = 4,
                PricePerHour = 100,
                FacilityId = 1,
                SportId = 1
            }
        };

        var courtDtos = new List<CourtDto>
        {
            new()
            {
                Id = 1,
                Name = "Tennis Court 1",
                Capacity = 4,
                PricePerHour = 100,
                FacilityId = 1,
                SportId = 1
            },
            new()
            {
                Id = 2,
                Name = "Tennis Court 2",
                Capacity = 4,
                PricePerHour = 100,
                FacilityId = 1,
                SportId = 1
            }
        };

        var mockRepo = new Mock<IGenericRepository<Court>>();
        mockRepo.Setup(r => r.GetAllWithSpecAsync(It.IsAny<CourtWithFiltersSpecification>()))
            .ReturnsAsync(courts);

        _mockUnitOfWork.Setup(uow => uow.Repository<Court>())
            .Returns(mockRepo.Object);

        _mockMapper.Setup(m => m.Map<IReadOnlyList<CourtDto>>(courts))
            .Returns(courtDtos);

        // Act
        var result = await _controller.GetAllCourts(specParams);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<CourtDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public async Task UpdateCourt_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var courtDto = new CourtDto
        {
            Id = 1,
            Name = "Updated Tennis Court",
            Capacity = 6,
            PricePerHour = 150,
            FacilityId = 1,
            SportId = 1
        };

        var existingCourt = new Court
        {
            Id = 1,
            Name = "Tennis Court 1",
            Capacity = 4,
            PricePerHour = 100,
            FacilityId = 1,
            SportId = 1
        };

        var mockRepo = new Mock<IGenericRepository<Court>>();
        mockRepo.Setup(r => r.GetByIdAsync(courtDto.Id))
            .ReturnsAsync(existingCourt);

        _mockUnitOfWork.Setup(uow => uow.Repository<Court>())
            .Returns(mockRepo.Object);
        _mockUnitOfWork.Setup(uow => uow.Complete())
            .ReturnsAsync(1);

        // Act
        var result = await _controller.UpdateCourt(courtDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Court updated successfully", okResult.Value);
        Assert.Equal(courtDto.Name, existingCourt.Name);
        Assert.Equal(courtDto.Capacity, existingCourt.Capacity);
        Assert.Equal(courtDto.PricePerHour, existingCourt.PricePerHour);
        Assert.Equal(courtDto.FacilityId, existingCourt.FacilityId);
        Assert.Equal(courtDto.SportId, existingCourt.SportId);
    }

    [Fact]
    public async Task UpdateCourt_WhenCourtNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var courtDto = new CourtDto
        {
            Id = 999,
            Name = "Updated Tennis Court",
            Capacity = 6,
            PricePerHour = 150,
            FacilityId = 1,
            SportId = 1
        };

        var mockRepo = new Mock<IGenericRepository<Court>>();
        mockRepo.Setup(r => r.GetByIdAsync(courtDto.Id))
            .ReturnsAsync((Court)null);

        _mockUnitOfWork.Setup(uow => uow.Repository<Court>())
            .Returns(mockRepo.Object);

        // Act
        var result = await _controller.UpdateCourt(courtDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(notFoundResult.Value);
        Assert.Equal(404, apiResponse.StatusCode);
        Assert.Equal("Court not found", apiResponse.Messege);
    }

    [Fact]
    public async Task GetCourtById_WhenCourtExists_ShouldReturnCourt()
    {
        // Arrange
        var courtId = 1;
        var court = new Court
        {
            Id = courtId,
            Name = "Tennis Court 1",
            Capacity = 4,
            PricePerHour = 100,
            FacilityId = 1,
            SportId = 1
        };

        var mockRepo = new Mock<IGenericRepository<Court>>();
        mockRepo.Setup(r => r.GetByIdAsync(courtId))
            .ReturnsAsync(court);

        _mockUnitOfWork.Setup(uow => uow.Repository<Court>())
            .Returns(mockRepo.Object);

        // Act
        var result = await _controller.GetCourtById(courtId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<Court>(okResult.Value);
        Assert.Equal(courtId, returnValue.Id);
        Assert.Equal(court.Name, returnValue.Name);
    }

    [Fact]
    public async Task GetCourtById_WhenCourtDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var courtId = 999;
        var mockRepo = new Mock<IGenericRepository<Court>>();
        mockRepo.Setup(r => r.GetByIdAsync(courtId))
            .ReturnsAsync((Court)null);

        _mockUnitOfWork.Setup(uow => uow.Repository<Court>())
            .Returns(mockRepo.Object);

        // Act
        var result = await _controller.GetCourtById(courtId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(notFoundResult.Value);
        Assert.Equal(404, apiResponse.StatusCode);
        Assert.Equal("Court not found", apiResponse.Messege);
    }
} 