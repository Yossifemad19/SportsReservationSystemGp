using System.Security.Claims;
using backend.Api.Controllers;
using backend.Api.DTOs;
using backend.Api.Services;
using backend.Api.Services.Interfaces;
using backend.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace backend.Tests.Controllers;

public class FriendRequestControllerTests
{
    private readonly Mock<IFriendRequestService> _mockFriendService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly FriendRequestController _controller;

    public FriendRequestControllerTests()
    {
        _mockFriendService = new Mock<IFriendRequestService>();
        _mockAuthService = new Mock<IAuthService>();
        _controller = new FriendRequestController(_mockFriendService.Object, _mockAuthService.Object);
    }

    private void SetupUserClaims(int userId)
    {
        var claims = new List<Claim>
        {
            new("sub", userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task SendFriendRequest_WithValidData_ShouldReturnOk()
    {
        // Arrange
        SetupUserClaims(1);
        var receiverId = 2;
        var expectedResult = (true, "Friend request sent successfully");

        _mockFriendService.Setup(service => service.SendFriendRequestAsync(1, receiverId))
            .Returns(Task.FromResult(expectedResult));

        // Act
        var result = await _controller.SendFriendRequest(receiverId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedResult.Item2, okResult.Value);
    }

    [Fact]
    public async Task SendFriendRequest_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        SetupUserClaims(1);
        var receiverId = 2;
        var expectedResult = (false, "Cannot send friend request");

        _mockFriendService.Setup(service => service.SendFriendRequestAsync(1, receiverId))
            .Returns(Task.FromResult(expectedResult));

        // Act
        var result = await _controller.SendFriendRequest(receiverId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(expectedResult.Item2, badRequestResult.Value);
    }

    [Fact]
    public async Task SendFriendRequest_WithInvalidUserId_ShouldReturnBadRequest()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _controller.SendFriendRequest(2);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid sender ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task RejectFriendRequest_WithValidData_ShouldReturnOk()
    {
        // Arrange
        SetupUserClaims(1);
        var requestId = 1;
        var expectedResult = (true, "Friend request rejected successfully");

        _mockFriendService.Setup(service => service.RejectFriendRequestAsync(requestId, 1))
            .Returns(Task.FromResult(expectedResult));

        // Act
        var result = await _controller.RejectFriendRequest(requestId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedResult.Item2, okResult.Value);
    }

    [Fact]
    public async Task AcceptFriendRequest_WithValidData_ShouldReturnOk()
    {
        // Arrange
        SetupUserClaims(1);
        var requestId = 1;
        var expectedResult = (true, "Friend request accepted successfully");

        _mockFriendService.Setup(service => service.AcceptFriendRequestAsync(requestId, 1))
            .Returns(Task.FromResult(expectedResult));

        // Act
        var result = await _controller.AcceptFriendRequest(requestId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedResult.Item2, okResult.Value);
    }

    [Fact]
    public async Task GetAcceptedFriendRequests_ShouldReturnAcceptedRequests()
    {
        // Arrange
        SetupUserClaims(1);
        var expectedRequests = new List<FriendRequestDto>
        {
            new() { Id = 1, SenderId = 2, ReceiverId = 1, Status = FriendRequestStatus.Accepted.ToString() },
            new() { Id = 2, SenderId = 3, ReceiverId = 1, Status = FriendRequestStatus.Accepted.ToString() }
        };

        _mockFriendService.Setup(service => service.GetAcceptedFriendRequestsAsync(1))
            .Returns(Task.FromResult(expectedRequests));

        // Act
        var result = await _controller.GetAcceptedFriendRequests();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<FriendRequestDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public async Task GetPendingRequests_ShouldReturnPendingRequests()
    {
        // Arrange
        SetupUserClaims(1);
        var expectedRequests = new List<FriendRequestDto>
        {
            new() { Id = 1, SenderId = 2, ReceiverId = 1, Status = FriendRequestStatus.Pending.ToString() },
            new() { Id = 2, SenderId = 3, ReceiverId = 1, Status = FriendRequestStatus.Pending.ToString() }
        };

        _mockFriendService.Setup(service => service.GetPendingRequestsAsync(1))
            .Returns(Task.FromResult(expectedRequests));

        // Act
        var result = await _controller.GetPendingRequests();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<FriendRequestDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public async Task GetSentRequests_ShouldReturnSentRequests()
    {
        // Arrange
        SetupUserClaims(1);
        var expectedRequests = new List<FriendRequestDto>
        {
            new() { Id = 1, SenderId = 1, ReceiverId = 2, Status = FriendRequestStatus.Pending.ToString() },
            new() { Id = 2, SenderId = 1, ReceiverId = 3, Status = FriendRequestStatus.Pending.ToString() }
        };

        _mockFriendService.Setup(service => service.GetSentRequestsAsync(1))
            .Returns(Task.FromResult(expectedRequests));

        // Act
        var result = await _controller.GetSentRequests();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<FriendRequestDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public async Task GetReceivedRequests_ShouldReturnReceivedRequests()
    {
        // Arrange
        SetupUserClaims(1);
        var expectedRequests = new List<FriendRequestDto>
        {
            new() { Id = 1, SenderId = 2, ReceiverId = 1, Status = FriendRequestStatus.Pending.ToString() },
            new() { Id = 2, SenderId = 3, ReceiverId = 1, Status = FriendRequestStatus.Pending.ToString() }
        };

        _mockFriendService.Setup(service => service.GetReceivedRequestsAsync(1))
            .Returns(Task.FromResult(expectedRequests));

        // Act
        var result = await _controller.GetReceivedRequests();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<FriendRequestDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public async Task GetAcceptedFriendRequests_WithInvalidUserId_ShouldReturnBadRequest()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _controller.GetAcceptedFriendRequests();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid user ID.", badRequestResult.Value);
    }
} 