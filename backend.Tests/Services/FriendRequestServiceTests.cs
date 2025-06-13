using AutoMapper;
using backend.Api.DTOs;
using backend.Api.Hubs;
using backend.Api.Services;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.Services
{
    public class FriendRequestServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
        private readonly Mock<IClientProxy> _mockClientProxy;
        private readonly FriendRequestService _friendRequestService;

        public FriendRequestServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockHubContext = new Mock<IHubContext<NotificationHub>>();
            _mockClientProxy = new Mock<IClientProxy>();

            _mockHubContext.Setup(h => h.Clients.User(It.IsAny<string>()))
                .Returns(_mockClientProxy.Object);

            _friendRequestService = new FriendRequestService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockHubContext.Object
            );
        }

        [Fact]
        public async Task SendFriendRequestAsync_WithValidRequest_ShouldSucceed()
        {
            // Arrange
            var senderId = 1;
            var receiverId = 2;
            var mockRepo = new Mock<IGenericRepository<FriendRequest>>();

            _mockUnitOfWork.Setup(uow => uow.Repository<FriendRequest>())
                .Returns(mockRepo.Object);

            mockRepo.Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<FriendRequest, bool>>>()))
                .ReturnsAsync((FriendRequest?)null);

            _mockUnitOfWork.Setup(uow => uow.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _friendRequestService.SendFriendRequestAsync(senderId, receiverId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Friend request sent.", result.Message);
            mockRepo.Verify(repo => repo.Add(It.IsAny<FriendRequest>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.Complete(), Times.Once);
            _mockHubContext.Verify(h => h.Clients.User(receiverId.ToString()), Times.Once);
        }

        [Fact]
        public async Task SendFriendRequestAsync_ToSelf_ShouldFail()
        {
            // Arrange
            var userId = 1;

            // Act
            var result = await _friendRequestService.SendFriendRequestAsync(userId, userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You cannot send a friend request to yourself.", result.Message);
        }

        [Fact]
        public async Task SendFriendRequestAsync_WithExistingRequest_ShouldFail()
        {
            // Arrange
            var senderId = 1;
            var receiverId = 2;
            var mockRepo = new Mock<IGenericRepository<FriendRequest>>();
            var existingRequest = new FriendRequest { SenderId = senderId, ReceiverId = receiverId };

            _mockUnitOfWork.Setup(uow => uow.Repository<FriendRequest>())
                .Returns(mockRepo.Object);

            mockRepo.Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<FriendRequest, bool>>>()))
                .ReturnsAsync(existingRequest);

            // Act
            var result = await _friendRequestService.SendFriendRequestAsync(senderId, receiverId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("A friend request already exists between these users.", result.Message);
        }

        [Fact]
        public async Task AcceptFriendRequestAsync_WithValidRequest_ShouldSucceed()
        {
            // Arrange
            var requestId = 1;
            var userId = 2;
            var mockRepo = new Mock<IGenericRepository<FriendRequest>>();
            var request = new FriendRequest
            {
                Id = requestId,
                ReceiverId = userId,
                Status = FriendRequestStatus.Pending
            };

            _mockUnitOfWork.Setup(uow => uow.Repository<FriendRequest>())
                .Returns(mockRepo.Object);

            mockRepo.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);

            _mockUnitOfWork.Setup(uow => uow.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _friendRequestService.AcceptFriendRequestAsync(requestId, userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Friend request accepted.", result.Message);
            Assert.Equal(FriendRequestStatus.Accepted, request.Status);
            mockRepo.Verify(repo => repo.Update(It.IsAny<FriendRequest>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.Complete(), Times.Once);
            _mockHubContext.Verify(h => h.Clients.User(request.SenderId.ToString()), Times.Once);
        }

        [Fact]
        public async Task RejectFriendRequestAsync_WithValidRequest_ShouldSucceed()
        {
            // Arrange
            var requestId = 1;
            var userId = 2;
            var mockRepo = new Mock<IGenericRepository<FriendRequest>>();
            var request = new FriendRequest
            {
                Id = requestId,
                ReceiverId = userId,
                Status = FriendRequestStatus.Pending
            };

            _mockUnitOfWork.Setup(uow => uow.Repository<FriendRequest>())
                .Returns(mockRepo.Object);

            mockRepo.Setup(repo => repo.GetByIdAsync(requestId))
                .ReturnsAsync(request);

            _mockUnitOfWork.Setup(uow => uow.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _friendRequestService.RejectFriendRequestAsync(requestId, userId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Friend request rejected.", result.Message);
            Assert.Equal(FriendRequestStatus.Rejected, request.Status);
            mockRepo.Verify(repo => repo.Update(It.IsAny<FriendRequest>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.Complete(), Times.Once);
            _mockHubContext.Verify(h => h.Clients.User(request.SenderId.ToString()), Times.Once);
        }

        [Fact]
        public async Task GetPendingRequestsAsync_ShouldReturnMappedRequests()
        {
            // Arrange
            var userId = 1;
            var mockRepo = new Mock<IGenericRepository<FriendRequest>>();
            var requests = new List<FriendRequest>
            {
                new FriendRequest { Id = 1, SenderId = 2, ReceiverId = userId, Status = FriendRequestStatus.Pending },
                new FriendRequest { Id = 2, SenderId = 3, ReceiverId = userId, Status = FriendRequestStatus.Pending }
            };
            var expectedDtos = new List<FriendRequestDto>
            {
                new FriendRequestDto { Id = 1 },
                new FriendRequestDto { Id = 2 }
            };

            _mockUnitOfWork.Setup(uow => uow.Repository<FriendRequest>())
                .Returns(mockRepo.Object);

            mockRepo.Setup(repo => repo.GetAllWithSpecAsync(It.IsAny<FriendRequestSpec>()))
                .ReturnsAsync(requests);

            _mockMapper.Setup(m => m.Map<List<FriendRequestDto>>(requests))
                .Returns(expectedDtos);

            // Act
            var result = await _friendRequestService.GetPendingRequestsAsync(userId);

            // Assert
            Assert.Equal(expectedDtos, result);
            mockRepo.Verify(repo => repo.GetAllWithSpecAsync(It.IsAny<FriendRequestSpec>()), Times.Once);
            _mockMapper.Verify(m => m.Map<List<FriendRequestDto>>(requests), Times.Once);
        }

        [Fact]
        public async Task AreFriendsAsync_WithExistingFriendship_ShouldReturnTrue()
        {
            // Arrange
            var userId1 = 1;
            var userId2 = 2;
            var mockRepo = new Mock<IGenericRepository<FriendRequest>>();
            var friendship = new FriendRequest
            {
                SenderId = userId1,
                ReceiverId = userId2,
                Status = FriendRequestStatus.Accepted
            };

            _mockUnitOfWork.Setup(uow => uow.Repository<FriendRequest>())
                .Returns(mockRepo.Object);

            mockRepo.Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<FriendRequest, bool>>>()))
                .ReturnsAsync(friendship);

            // Act
            var result = await _friendRequestService.AreFriendsAsync(userId1, userId2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AreFriendsAsync_WithNoFriendship_ShouldReturnFalse()
        {
            // Arrange
            var userId1 = 1;
            var userId2 = 2;
            var mockRepo = new Mock<IGenericRepository<FriendRequest>>();

            _mockUnitOfWork.Setup(uow => uow.Repository<FriendRequest>())
                .Returns(mockRepo.Object);

            mockRepo.Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<FriendRequest, bool>>>()))
                .ReturnsAsync((FriendRequest?)null);

            // Act
            var result = await _friendRequestService.AreFriendsAsync(userId1, userId2);

            // Assert
            Assert.False(result);
        }
    }
} 