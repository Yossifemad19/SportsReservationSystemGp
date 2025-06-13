using System.Security.Claims;
using backend.Api.Controllers;
using backend.Api.DTOs;
using backend.Api.Services;
using backend.Api.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace backend.Tests.Controllers;

public class FacilitiesControllerTests
{
    private readonly Mock<IFacilityService> _mockFacilityService;
    private readonly Mock<ICityService> _mockCityService;
    private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
    private readonly FacilitiesController _controller;

    public FacilitiesControllerTests()
    {
        _mockFacilityService = new Mock<IFacilityService>();
        _mockCityService = new Mock<ICityService>();
        _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        _controller = new FacilitiesController(_mockFacilityService.Object, _mockWebHostEnvironment.Object, _mockCityService.Object);
    }

    [Fact]
    public async Task Get_WhenFacilityExists_ShouldReturnOk()
    {
        // Arrange
        var facilityId = 1;
        var facilityDto = new FacilityDto
        {
            Id = facilityId,
            Name = "Test Facility",
            Address = new AddressDto { StreetAddress = "123 Test St", City = "Test City", Latitude = 0, Longitude = 0 },
            OpeningTime = TimeSpan.FromHours(9),
            ClosingTime = TimeSpan.FromHours(17),
            OwnerId = 1
        };

        _mockFacilityService.Setup(service => service.GetFacilityById(facilityId))
            .Returns(Task.FromResult(ServiceResult<FacilityDto>.Ok(facilityDto)));

        // Act
        var result = await _controller.Get(facilityId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<FacilityDto>>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.Equal(facilityId, returnValue.Data.Id);
    }

    [Fact]
    public async Task Get_WhenFacilityDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var facilityId = 999;
        _mockFacilityService.Setup(service => service.GetFacilityById(facilityId))
            .Returns(Task.FromResult(ServiceResult<FacilityDto>.Fail("Facility not found")));

        // Act
        var result = await _controller.Get(facilityId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<FacilityDto>>(notFoundResult.Value);
        Assert.False(returnValue.Success);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var ownerId = "1";
        var facilityDto = new FacilityDto
        {
            Name = "New Facility",
            Address = new AddressDto { StreetAddress = "456 New St", City = "New City", Latitude = 0, Longitude = 0 },
            OpeningTime = TimeSpan.FromHours(8),
            ClosingTime = TimeSpan.FromHours(20),
            OwnerId = 1
        };

        var claims = new List<Claim>
        {
            new Claim("sub", ownerId),
            new Claim(ClaimTypes.Role, "Owner")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockFacilityService.Setup(service => service.CreateFacility(facilityDto, ownerId))
            .Returns(Task.FromResult(ServiceResult<FacilityDto>.Ok(facilityDto)));

        // Act
        var result = await _controller.Create(facilityDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<FacilityDto>>(okResult.Value);
        Assert.True(returnValue.Success);
    }

    [Fact]
    public async Task Create_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var ownerId = "1";
        var facilityDto = new FacilityDto
        {
            Name = "New Facility",
            Address = new AddressDto { StreetAddress = "456 New St", City = "New City", Latitude = 0, Longitude = 0 },
            OpeningTime = TimeSpan.FromHours(8),
            ClosingTime = TimeSpan.FromHours(20),
            OwnerId = 1
        };

        var claims = new List<Claim>
        {
            new Claim("sub", ownerId),
            new Claim(ClaimTypes.Role, "Owner")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockFacilityService.Setup(service => service.CreateFacility(facilityDto, ownerId))
            .Returns(Task.FromResult(ServiceResult<FacilityDto>.Fail("Invalid facility data")));

        // Act
        var result = await _controller.Create(facilityDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<FacilityDto>>(badRequestResult.Value);
        Assert.False(returnValue.Success);
    }

    [Fact]
    public async Task UpdateFacility_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var ownerId = "1";
        var facilityDto = new FacilityDto
        {
            Id = 1,
            Name = "Updated Facility",
            Address = new AddressDto { StreetAddress = "789 Update St", City = "Update City", Latitude = 0, Longitude = 0 },
            OpeningTime = TimeSpan.FromHours(7),
            ClosingTime = TimeSpan.FromHours(21),
            OwnerId = 1
        };

        var claims = new List<Claim>
        {
            new Claim("sub", ownerId),
            new Claim(ClaimTypes.Role, "Owner")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockFacilityService.Setup(service => service.UpdateFacility(facilityDto, ownerId))
            .Returns(Task.FromResult(ServiceResult<FacilityDto>.Ok(facilityDto)));

        // Act
        var result = await _controller.UpdateFacility(facilityDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<FacilityDto>>(okResult.Value);
        Assert.True(returnValue.Success);
    }

    [Fact]
    public async Task UpdateFacility_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var ownerId = "1";
        var facilityDto = new FacilityDto
        {
            Id = 0, // Invalid ID
            Name = "Updated Facility",
            Address = new AddressDto { StreetAddress = "789 Update St", City = "Update City", Latitude = 0, Longitude = 0 },
            OpeningTime = TimeSpan.FromHours(7),
            ClosingTime = TimeSpan.FromHours(21),
            OwnerId = 1
        };

        var claims = new List<Claim>
        {
            new Claim("sub", ownerId),
            new Claim(ClaimTypes.Role, "Owner")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _controller.UpdateFacility(facilityDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<FacilityDto>>(badRequestResult.Value);
        Assert.False(returnValue.Success);
    }

    [Fact]
    public async Task Delete_WhenFacilityExists_ShouldReturnOk()
    {
        // Arrange
        var facilityId = 1;
        var ownerId = "1";

        var claims = new List<Claim>
        {
            new Claim("sub", ownerId),
            new Claim(ClaimTypes.Role, "Owner")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockFacilityService.Setup(service => service.DeleteFacility(facilityId))
            .Returns(Task.FromResult(ServiceResult<bool>.Ok(true)));

        // Act
        var result = await _controller.Delete(facilityId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<bool>>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.True(returnValue.Data);
    }

    [Fact]
    public async Task Delete_WhenFacilityDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var facilityId = 999;
        var ownerId = "1";

        var claims = new List<Claim>
        {
            new Claim("sub", ownerId),
            new Claim(ClaimTypes.Role, "Owner")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockFacilityService.Setup(service => service.DeleteFacility(facilityId))
            .Returns(Task.FromResult(ServiceResult<bool>.Fail("Facility not found")));

        // Act
        var result = await _controller.Delete(facilityId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<bool>>(notFoundResult.Value);
        Assert.False(returnValue.Success);
    }

    [Fact]
    public async Task GetAllFacilities_WithNoFilters_ShouldReturnOk()
    {
        // Arrange
        var facilities = new List<FacilityDto>
        {
            new()
            {
                Id = 1,
                Name = "Facility 1",
                Address = new AddressDto { StreetAddress = "123 Test St", City = "Test City", Latitude = 0, Longitude = 0 },
                OpeningTime = TimeSpan.FromHours(9),
                ClosingTime = TimeSpan.FromHours(17),
                OwnerId = 1
            },
            new()
            {
                Id = 2,
                Name = "Facility 2",
                Address = new AddressDto { StreetAddress = "456 Test St", City = "Test City", Latitude = 0, Longitude = 0 },
                OpeningTime = TimeSpan.FromHours(8),
                ClosingTime = TimeSpan.FromHours(18),
                OwnerId = 2
            }
        };

        var claims = new List<Claim>
        {
            new Claim("sub", "1"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockFacilityService.Setup(service => service.GetAllFacilities(false, "1", null, null))
            .Returns(Task.FromResult(ServiceResult<List<FacilityDto>>.Ok(facilities)));

        // Act
        var result = await _controller.GetAllFacilities();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<List<FacilityDto>>>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.NotNull(returnValue.Data);
        Assert.Equal(2, returnValue.Data.Count);
    }

    [Fact]
    public async Task GetAllFacilities_WithFilters_ShouldReturnOk()
    {
        // Arrange
        var ownerId = "1";
        var sportId = 1;
        var city = "Test City";
        var facilities = new List<FacilityDto>
        {
            new()
            {
                Id = 1,
                Name = "Facility 1",
                Address = new AddressDto { StreetAddress = "123 Test St", City = "Test City", Latitude = 0, Longitude = 0 },
                OpeningTime = TimeSpan.FromHours(9),
                ClosingTime = TimeSpan.FromHours(17),
                OwnerId = 1
            }
        };

        var claims = new List<Claim>
        {
            new Claim("sub", ownerId),
            new Claim(ClaimTypes.Role, "Owner")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockFacilityService.Setup(service => service.GetAllFacilities(true, ownerId, sportId, city))
            .Returns(Task.FromResult(ServiceResult<List<FacilityDto>>.Ok(facilities)));

        // Act
        var result = await _controller.GetAllFacilities(true, sportId, city);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<List<FacilityDto>>>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.NotNull(returnValue.Data);
        Assert.Single(returnValue.Data);
    }

    [Fact]
    public void GetAllCities_ShouldReturnOk()
    {
        // Arrange
        var cities = new List<string> { "City 1", "City 2", "City 3" };
        _mockCityService.Setup(service => service.GetAllAllowedCities())
            .Returns(cities);

        // Act
        var result = _controller.GetAllCities();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<IReadOnlyCollection<string>>>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.Equal(3, returnValue.Data.Count);
    }
} 