using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Api.DTOs;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Tests.Services
{
    public class MatchServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<MatchService>> _mockLogger;
        private readonly Mock<IGenericRepository<backend.Core.Entities.Match>> _mockMatchRepo;
        private readonly Mock<IGenericRepository<MatchPlayer>> _mockMatchPlayerRepo;
        private readonly Mock<IGenericRepository<PlayerRating>> _mockPlayerRatingRepo;
        private readonly Mock<IGenericRepository<Booking>> _mockBookingRepo;
        private readonly Mock<IGenericRepository<PlayerProfile>> _mockPlayerProfileRepo;
        private readonly MatchService _matchService;

        public MatchServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<MatchService>>();
            _mockMatchRepo = new Mock<IGenericRepository<backend.Core.Entities.Match>>();
            _mockMatchPlayerRepo = new Mock<IGenericRepository<MatchPlayer>>();
            _mockPlayerRatingRepo = new Mock<IGenericRepository<PlayerRating>>();
            _mockBookingRepo = new Mock<IGenericRepository<Booking>>();
            _mockPlayerProfileRepo = new Mock<IGenericRepository<PlayerProfile>>();

            _mockUnitOfWork.Setup(x => x.Repository<backend.Core.Entities.Match>()).Returns(_mockMatchRepo.Object);
            _mockUnitOfWork.Setup(x => x.Repository<MatchPlayer>()).Returns(_mockMatchPlayerRepo.Object);
            _mockUnitOfWork.Setup(x => x.Repository<PlayerRating>()).Returns(_mockPlayerRatingRepo.Object);
            _mockUnitOfWork.Setup(x => x.Repository<Booking>()).Returns(_mockBookingRepo.Object);
            _mockUnitOfWork.Setup(x => x.Repository<PlayerProfile>()).Returns(_mockPlayerProfileRepo.Object);

            _matchService = new MatchService(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateMatchAsync_ValidData_ReturnsMatch()
        {
            // Arrange
            var booking = new Booking { Id = 1, UserId = 1 };
            var match = new backend.Core.Entities.Match
            {
                Id = 1,
                CreatorUserId = 1,
                BookingId = 1,
                SportId = 1,
                TeamSize = 5,
                Title = "Test Match",
                Description = "Test Description",
                Status = MatchStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            _mockBookingRepo.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Booking, bool>>>()))
                .ReturnsAsync(booking);

            _mockMatchRepo.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<backend.Core.Entities.Match, bool>>>()))
                .ReturnsAsync((backend.Core.Entities.Match)null);

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _matchService.CreateMatchAsync(1, 1, 1, 5, "Test Match", "Test Description", null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CreatorUserId);
            Assert.Equal(1, result.BookingId);
            Assert.Equal(1, result.SportId);
            Assert.Equal(5, result.TeamSize);
            Assert.Equal("Test Match", result.Title);
            Assert.Equal(MatchStatus.Open, result.Status);
        }

        [Fact]
        public async Task CreateMatchAsync_InvalidBooking_ThrowsException()
        {
            // Arrange
            _mockBookingRepo.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Booking, bool>>>()))
                .ReturnsAsync((Booking)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _matchService.CreateMatchAsync(1, 1, 1, 5, "Test Match", "Test Description", null, null));
        }

        [Fact]
        public async Task GetMatchByIdAsync_ValidId_ReturnsMatch()
        {
            // Arrange
            var match = new backend.Core.Entities.Match
            {
                Id = 1,
                CreatorUserId = 1,
                BookingId = 1,
                SportId = 1,
                TeamSize = 5,
                Title = "Test Match",
                Description = "Test Description",
                Status = MatchStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            _mockMatchRepo.Setup(x => x.GetByIdWithSpecAsync(It.IsAny<Core.Specification.ISpecification<backend.Core.Entities.Match>>()))
                .ReturnsAsync(match);

            // Act
            var result = await _matchService.GetMatchByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Match", result.Title);
        }

        [Fact]
        public async Task GetMatchesByUserIdAsync_ReturnsListOfMatches()
        {
            // Arrange
            var matches = new List<backend.Core.Entities.Match>
            {
                new backend.Core.Entities.Match
                {
                    Id = 1,
                    CreatorUserId = 1,
                    BookingId = 1,
                    SportId = 1,
                    TeamSize = 5,
                    Title = "Test Match 1",
                    Status = MatchStatus.Open,
                    CreatedAt = DateTime.UtcNow
                },
                new backend.Core.Entities.Match
                {
                    Id = 2,
                    CreatorUserId = 1,
                    BookingId = 2,
                    SportId = 2,
                    TeamSize = 3,
                    Title = "Test Match 2",
                    Status = MatchStatus.Open,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockMatchRepo.Setup(x => x.GetAllWithSpecAsync(It.IsAny<Core.Specification.ISpecification<backend.Core.Entities.Match>>()))
                .ReturnsAsync(matches);

            // Act
            var result = await _matchService.GetMatchesByUserIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.Title == "Test Match 1");
            Assert.Contains(result, m => m.Title == "Test Match 2");
        }

        [Fact]
        public async Task InvitePlayerToMatchAsync_ValidData_ReturnsTrue()
        {
            // Arrange
            var match = new backend.Core.Entities.Match
            {
                Id = 1,
                CreatorUserId = 1,
                Status = MatchStatus.Open,
                TeamSize = 5
            };

            _mockMatchRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(match);

            _mockMatchPlayerRepo.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<MatchPlayer, bool>>>()))
                .ReturnsAsync((MatchPlayer)null);

            _mockMatchPlayerRepo.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<MatchPlayer>());

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _matchService.InvitePlayerToMatchAsync(1, 2, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RespondToInvitationAsync_ValidData_ReturnsTrue()
        {
            // Arrange
            var match = new backend.Core.Entities.Match
            {
                Id = 1,
                Status = MatchStatus.Open
            };

            var invitation = new MatchPlayer
            {
                MatchId = 1,
                UserId = 2,
                Status = ParticipationStatus.Invited
            };

            _mockMatchRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(match);

            _mockMatchPlayerRepo.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<MatchPlayer, bool>>>()))
                .ReturnsAsync(invitation);

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _matchService.RespondToInvitationAsync(1, 2, true);

            // Assert
            Assert.True(result);
            Assert.Equal(ParticipationStatus.Accepted, invitation.Status);
        }

        [Fact]
        public async Task RatePlayerAsync_ValidData_ReturnsTrue()
        {
            // Arrange
            var match = new backend.Core.Entities.Match
            {
                Id = 1,
                Status = MatchStatus.Completed
            };

            var rater = new MatchPlayer
            {
                MatchId = 1,
                UserId = 1,
                Status = ParticipationStatus.CheckedIn
            };

            var rated = new MatchPlayer
            {
                MatchId = 1,
                UserId = 2,
                Status = ParticipationStatus.CheckedIn
            };

            var playerProfile = new PlayerProfile
            {
                UserId = 2,
                SkillLevel = 5,
                MatchesPlayed = 0
            };

            _mockMatchRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(match);

            _mockMatchPlayerRepo.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<MatchPlayer, bool>>>()))
                .ReturnsAsync(rater);

            _mockPlayerProfileRepo.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<PlayerProfile, bool>>>()))
                .ReturnsAsync(playerProfile);

            _mockPlayerRatingRepo.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<PlayerRating>());

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _matchService.RatePlayerAsync(1, 1, 2, 4, 5, "Great player!");

            // Assert
            Assert.True(result);
            Assert.Equal(1, playerProfile.MatchesPlayed);
        }

        [Fact]
        public async Task StartMatchAsync_ValidData_ReturnsTrue()
        {
            // Arrange
            var match = new backend.Core.Entities.Match
            {
                Id = 1,
                CreatorUserId = 1,
                Status = MatchStatus.Open
            };

            var players = new List<MatchPlayer>
            {
                new MatchPlayer { MatchId = 1, UserId = 1, Status = ParticipationStatus.CheckedIn },
                new MatchPlayer { MatchId = 1, UserId = 2, Status = ParticipationStatus.CheckedIn },
                new MatchPlayer { MatchId = 1, UserId = 3, Status = ParticipationStatus.CheckedIn },
                new MatchPlayer { MatchId = 1, UserId = 4, Status = ParticipationStatus.CheckedIn }
            };

            _mockMatchRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(match);

            _mockMatchPlayerRepo.Setup(x => x.GetAllAsync())
                .ReturnsAsync(players);

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _matchService.StartMatchAsync(1, 1);

            // Assert
            Assert.True(result);
            Assert.Equal(MatchStatus.InProgress, match.Status);
        }

        [Fact]
        public async Task CancelMatchAsync_ValidData_ReturnsTrue()
        {
            // Arrange
            var match = new backend.Core.Entities.Match
            {
                Id = 1,
                CreatorUserId = 1,
                Status = MatchStatus.Open
            };

            _mockMatchRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(match);

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _matchService.CancelMatchAsync(1, 1);

            // Assert
            Assert.True(result);
            Assert.Equal(MatchStatus.Cancelled, match.Status);
        }

        [Fact]
        public async Task CompleteMatchAsync_ValidData_ReturnsTrue()
        {
            // Arrange
            var match = new backend.Core.Entities.Match
            {
                Id = 1,
                Status = MatchStatus.InProgress
            };

            _mockMatchRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(match);

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _matchService.CompleteMatchAsync(1);

            // Assert
            Assert.True(result);
            Assert.Equal(MatchStatus.Completed, match.Status);
            Assert.NotNull(match.CompletedAt);
        }
    }
} 