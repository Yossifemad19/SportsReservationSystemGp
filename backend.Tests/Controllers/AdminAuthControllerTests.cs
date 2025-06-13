using backend.Api.Controllers;
using backend.Api.DTOs;
using backend.Api.Services;
using backend.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.Controllers
{
    public class AdminAuthControllerTests
    {
        private readonly Mock<IAdminService> _mockAdminService;
        private readonly AdminAuthController _controller;

        public AdminAuthControllerTests()
        {
            _mockAdminService = new Mock<IAdminService>();
            _controller = new AdminAuthController(_mockAdminService.Object);
        }

        [Fact]
        public async Task AdminLogin_WithValidCredentials_ShouldReturnOk()
        {
            // Arrange
            var loginDto = new AdminLoginDto { Email = "admin@test.com", Password = "password123" };
            var expectedResponse = new ServiceResult<UserResponseDto> { Success = true, Data = new UserResponseDto { Id = 1, Email = "admin@test.com" } };

            _mockAdminService.Setup(service => service.AdminLogin(loginDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.AdminLogin(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ServiceResult<UserResponseDto>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(expectedResponse.Data.Email, returnValue.Data.Email);
        }

        [Fact]
        public async Task AdminLogin_WithInvalidCredentials_ShouldReturnBadRequest()
        {
            // Arrange
            var loginDto = new AdminLoginDto { Email = "admin@test.com", Password = "wrongpassword" };
            var expectedResponse = new ServiceResult<UserResponseDto> { Success = false, Message = "Invalid credentials" };

            _mockAdminService.Setup(service => service.AdminLogin(loginDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.AdminLogin(loginDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<ServiceResult<UserResponseDto>>(badRequestResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Invalid credentials", returnValue.Message);
        }

        [Fact]
        public async Task ApproveOwner_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var ownerId = 1;
            var expectedResponse = new ServiceResult<bool> { Success = true, Data = true };

            _mockAdminService.Setup(service => service.ApproveOwner(ownerId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ApproveOwner(ownerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ServiceResult<bool>>(okResult.Value);
            Assert.True(returnValue.Success);
        }

        [Fact]
        public async Task ApproveOwner_WithInvalidId_ShouldReturnBadRequest()
        {
            // Arrange
            var ownerId = 999;
            var expectedResponse = new ServiceResult<bool> { Success = false, Message = "Owner not found" };

            _mockAdminService.Setup(service => service.ApproveOwner(ownerId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ApproveOwner(ownerId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<ServiceResult<bool>>(badRequestResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("Owner not found", returnValue.Message);
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnOkWithUsers()
        {
            // Arrange
            var expectedResponse = new ServiceResult<List<GetAllResponse>>
            {
                Success = true,
                Data = new List<GetAllResponse>
                {
                    new GetAllResponse { Id = 1, Email = "user1@test.com" },
                    new GetAllResponse { Id = 2, Email = "user2@test.com" }
                }
            };

            _mockAdminService.Setup(service => service.GetAllUsers())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ServiceResult<List<GetAllResponse>>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(2, returnValue.Data.Count);
        }

        [Fact]
        public async Task GetAllOwners_ShouldReturnOkWithOwners()
        {
            // Arrange
            var expectedResponse = new ServiceResult<List<GetAllOwnerResponse>>
            {
                Success = true,
                Data = new List<GetAllOwnerResponse>
                {
                    new GetAllOwnerResponse { Id = 1, Email = "owner1@test.com" },
                    new GetAllOwnerResponse { Id = 2, Email = "owner2@test.com" }
                }
            };

            _mockAdminService.Setup(service => service.GetAllOwners())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAllOwners();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ServiceResult<List<GetAllOwnerResponse>>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(2, returnValue.Data.Count);
        }

        [Fact]
        public async Task RejectOwner_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var ownerId = 1;
            var expectedResponse = new ServiceResult<bool> { Success = true, Data = true };

            _mockAdminService.Setup(service => service.RejectOwner(ownerId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RejectOwner(ownerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ServiceResult<bool>>(okResult.Value);
            Assert.True(returnValue.Success);
        }

        [Fact]
        public async Task GetUnapprovedOwners_ShouldReturnOkWithUnapprovedOwners()
        {
            // Arrange
            var expectedResponse = new ServiceResult<IEnumerable<UnApprovedOwnerDto>>
            {
                Success = true,
                Data = new List<UnApprovedOwnerDto>
                {
                    new UnApprovedOwnerDto { FirstName = "John", LastName = "Doe", Email = "pending1@test.com", IsApproved = false },
                    new UnApprovedOwnerDto { FirstName = "Jane", LastName = "Smith", Email = "pending2@test.com", IsApproved = false }
                }
            };

            _mockAdminService.Setup(service => service.GetAllUnApprovedOwners())
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUnapprovedOwners();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ServiceResult<IEnumerable<UnApprovedOwnerDto>>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(2, returnValue.Data.Count());
            Assert.Contains(returnValue.Data, o => o.Email == "pending1@test.com" && !o.IsApproved);
            Assert.Contains(returnValue.Data, o => o.Email == "pending2@test.com" && !o.IsApproved);
        }

        [Fact]
        public async Task GetOwnerById_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var ownerId = 1;
            var expectedResponse = new ServiceResult<GetAllResponse>
            {
                Success = true,
                Data = new GetAllResponse { Id = 1, Email = "owner@test.com" }
            };

            _mockAdminService.Setup(service => service.GetOwnerById(ownerId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetOwnerById(ownerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ServiceResult<GetAllResponse>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(ownerId, returnValue.Data.Id);
        }

        [Fact]
        public async Task GetUserById_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var userId = 1;
            var expectedResponse = new ServiceResult<GetAllResponse>
            {
                Success = true,
                Data = new GetAllResponse { Id = 1, Email = "user@test.com" }
            };

            _mockAdminService.Setup(service => service.GetUserById(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ServiceResult<GetAllResponse>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(userId, returnValue.Data.Id);
        }

        [Fact]
        public async Task GetUserById_WithInvalidId_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = 999;
            var expectedResponse = new ServiceResult<GetAllResponse>
            {
                Success = false,
                Message = "User not found"
            };

            _mockAdminService.Setup(service => service.GetUserById(userId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<ServiceResult<GetAllResponse>>(badRequestResult.Value);
            Assert.False(returnValue.Success);
            Assert.Equal("User not found", returnValue.Message);
        }
    }
} 