using backend.Api.Controllers;
using backend.Api.DTOs;
using backend.Api.Services;
using backend.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace backend.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var registerDto = new RegisterDto
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

        _mockAuthService.Setup(service => service.Register(registerDto, "Customer"))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = okResult.Value;
        
        var message = returnValue?.GetType().GetProperty("message")?.GetValue(returnValue)?.ToString();
        var fullName = returnValue?.GetType().GetProperty("FullName")?.GetValue(returnValue)?.ToString();
        var email = returnValue?.GetType().GetProperty("Email")?.GetValue(returnValue)?.ToString();
        var phoneNumber = returnValue?.GetType().GetProperty("phoneNumber")?.GetValue(returnValue)?.ToString();
        var role = returnValue?.GetType().GetProperty("Role")?.GetValue(returnValue)?.ToString();
        var token = returnValue?.GetType().GetProperty("token")?.GetValue(returnValue)?.ToString();

        Assert.Equal("Registered successfully", message);
        Assert.Equal("John Doe", fullName);
        Assert.Equal("john@example.com", email);
        Assert.Equal("1234567890", phoneNumber);
        Assert.Equal("Customer", role);
        Assert.Equal(expectedToken, token);
    }

    [Fact]
    public async Task Register_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDto
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

        _mockAuthService.Setup(service => service.Register(registerDto, "Customer"))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<string>>(badRequestResult.Value);
        Assert.False(returnValue.Success);
        Assert.Equal("Invalid registration data", returnValue.Message);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "john@example.com",
            Password = "Password123!"
        };

        var expectedResponse = new ServiceResult<UserResponseDto>
        {
            Success = true,
            Data = new UserResponseDto
            {
                Email = "john@example.com",
                Token = "valid.jwt.token"
            }
        };

        _mockAuthService.Setup(service => service.Login(loginDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<UserResponseDto>>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.Equal("john@example.com", returnValue.Data?.Email);
        Assert.Equal("valid.jwt.token", returnValue.Data?.Token);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnBadRequest()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "john@example.com",
            Password = "WrongPassword123!"
        };

        var expectedResponse = new ServiceResult<UserResponseDto>
        {
            Success = false,
            Message = "Invalid credentials"
        };

        _mockAuthService.Setup(service => service.Login(loginDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<UserResponseDto>>(badRequestResult.Value);
        Assert.False(returnValue.Success);
        Assert.Equal("Invalid credentials", returnValue.Message);
    }

    [Fact]
    public async Task GetUserById_WithValidUserId_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var expectedResponse = new ServiceResult<GetAllResponse>
        {
            Success = true,
            Data = new GetAllResponse
            {
                Id = userId,
                Email = "john@example.com",
                FirstName = "John",
                LastName = "Doe"
            }
        };

        _mockAuthService.Setup(service => service.GetUserById(userId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetUserById();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<GetAllResponse>>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.Equal(userId, returnValue.Data?.Id);
        Assert.Equal("john@example.com", returnValue.Data?.Email);
    }

    [Fact]
    public async Task GetUserById_WithInvalidUserId_ShouldReturnBadRequest()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim("sub", "invalid-id")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _controller.GetUserById();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<GetAllResponse>>(badRequestResult.Value);
        Assert.False(returnValue.Success);
        Assert.Equal("Invalid or missing user ID.", returnValue.Message);
    }

    [Fact]
    public async Task UpdateUserProfile_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var userProfile = new UserProfileDto
        {
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "1234567890",
            Email = "john@example.com"
        };

        var expectedResponse = new ServiceResult<UserProfileDto>
        {
            Success = true,
            Data = userProfile,
            Message = "Profile updated successfully."
        };

        _mockAuthService.Setup(service => service.UpdateUserProfile(userId, userProfile))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateUserProfile(userProfile);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<UserProfileDto>>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.Equal("John", returnValue.Data?.FirstName);
        Assert.Equal("Doe", returnValue.Data?.LastName);
        Assert.Equal("1234567890", returnValue.Data?.PhoneNumber);
        Assert.Equal("john@example.com", returnValue.Data?.Email);
        Assert.Equal("Profile updated successfully.", returnValue.Message);
    }

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ShouldReturnOk()
    {
        // Arrange
        var forgotPasswordDto = new ForgotPasswordDto
        {
            Email = "john@example.com"
        };

        var expectedResponse = new ServiceResult<string>
        {
            Success = true,
            Message = "Password reset email sent successfully"
        };

        _mockAuthService.Setup(service => service.ForgotPassword(forgotPasswordDto.Email))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ForgotPassword(forgotPasswordDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<string>>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.Equal("Password reset email sent successfully", returnValue.Message);
    }

    [Fact]
    public async Task ResetPassword_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var passwordDto = new PasswordDto
        {
            Token = "valid.reset.token",
            NewPassword = "NewPassword123!"
        };

        var expectedResponse = new ServiceResult<string>
        {
            Success = true,
            Message = "Password reset successfully"
        };

        _mockAuthService.Setup(service => service.ResetPassword(passwordDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ResetPassword(passwordDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<string>>(okResult.Value);
        Assert.True(returnValue.Success);
        Assert.Equal("Password reset successfully", returnValue.Message);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var passwordDto = new PasswordDto
        {
            Token = "invalid.token",
            NewPassword = "weak"
        };

        var expectedResponse = new ServiceResult<string>
        {
            Success = false,
            Message = "Failed to reset password"
        };

        _mockAuthService.Setup(service => service.ResetPassword(passwordDto))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ResetPassword(passwordDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsType<ServiceResult<string>>(badRequestResult.Value);
        Assert.False(returnValue.Success);
        Assert.Equal("Failed to reset password", returnValue.Message);
    }
} 