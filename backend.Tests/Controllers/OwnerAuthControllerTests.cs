using backend.Api.Controllers;
using backend.Api.DTOs;
using backend.Api.Services;
using backend.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace backend.Tests.Controllers;

public class OwnerAuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly OwnerAuthController _controller;

    public OwnerAuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new OwnerAuthController(_mockAuthService.Object);
    }

    [Fact]
    public async Task OwnerRegister_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var registerDto = new OwnerRegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            Password = "Password123!"
        };

        var expectedToken = "valid.jwt.token";
        var expectedResponse = new ServiceResult<string>
        {
            Success = true,
            Data = expectedToken
        };

        _mockAuthService.Setup(service => service.OwnerRegister(registerDto, "Owner"))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.OwnerRegister(registerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = okResult.Value;
        
        // Use reflection to verify the anonymous type properties
        var message = returnValue.GetType().GetProperty("message")?.GetValue(returnValue)?.ToString();
        var token = returnValue.GetType().GetProperty("token")?.GetValue(returnValue)?.ToString();
        var owner = returnValue.GetType().GetProperty("owner")?.GetValue(returnValue);
        
        Assert.Equal("Registered successfully", message);
        Assert.Equal(expectedToken, token);
        
        // Verify owner properties
        var fullName = owner?.GetType().GetProperty("FullName")?.GetValue(owner)?.ToString();
        var email = owner?.GetType().GetProperty("Email")?.GetValue(owner)?.ToString();
        var phoneNumber = owner?.GetType().GetProperty("phoneNumber")?.GetValue(owner)?.ToString();
        var role = owner?.GetType().GetProperty("Role")?.GetValue(owner)?.ToString();
        
        Assert.Equal("John Doe", fullName);
        Assert.Equal("john@example.com", email);
        Assert.Equal("1234567890", phoneNumber);
        Assert.Equal("Owner", role);
    }

    [Fact]
    public async Task OwnerRegister_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var registerDto = new OwnerRegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email",
            PhoneNumber = "123",
            Password = "weak"
        };

        var expectedResponse = new ServiceResult<string>
        {
            Success = false,
            Message = "Invalid registration data"
        };

        _mockAuthService.Setup(service => service.OwnerRegister(registerDto, "Owner"))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.OwnerRegister(registerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<string>>(badRequestResult.Value);
        Assert.False(returnValue.Success);
        Assert.Equal("Invalid registration data", returnValue.Message);
    }

    [Fact]
    public async Task OwnerLogin_WithValidCredentialsAndApprovedAccount_ShouldReturnOk()
    {
        // Arrange
        var loginDto = new OwnerLoginDto
        {
            Email = "john@example.com",
            Password = "Password123!"
        };

        var expectedResponse = new ServiceResult<OwnerResponseDto>
        {
            Success = true,
            Data = new OwnerResponseDto
            {
                Email = "john@example.com",
                IsApproved = "true"
            }
        };

        _mockAuthService.Setup(service => service.OwnerLogin(loginDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.OwnerLogin(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<OwnerResponseDto>>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.Equal("john@example.com", returnValue.Data.Email);
        Assert.Equal("true", returnValue.Data.IsApproved);
    }

    [Fact]
    public async Task OwnerLogin_WithInvalidCredentials_ShouldReturnBadRequest()
    {
        // Arrange
        var loginDto = new OwnerLoginDto
        {
            Email = "john@example.com",
            Password = "WrongPassword123!"
        };

        var expectedResponse = new ServiceResult<OwnerResponseDto>
        {
            Success = false,
            Message = "Invalid credentials"
        };

        _mockAuthService.Setup(service => service.OwnerLogin(loginDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.OwnerLogin(loginDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<OwnerResponseDto>>(badRequestResult.Value);
        Assert.False(returnValue.Success);
        Assert.Equal("Invalid credentials", returnValue.Message);
    }

    [Fact]
    public async Task OwnerLogin_WithUnapprovedAccount_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new OwnerLoginDto
        {
            Email = "john@example.com",
            Password = "Password123!"
        };

        var expectedResponse = new ServiceResult<OwnerResponseDto>
        {
            Success = true,
            Data = new OwnerResponseDto
            {
                Email = "john@example.com",
                IsApproved = "false"
            }
        };

        _mockAuthService.Setup(service => service.OwnerLogin(loginDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.OwnerLogin(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<OwnerResponseDto>>(unauthorizedResult.Value);
        Assert.False(returnValue.Success);
        Assert.Equal("Your account is not approved yet. Please wait for admin approval.", returnValue.Message);
    }
} 