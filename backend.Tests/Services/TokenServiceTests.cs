using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Api.Services;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace backend.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            // Setup configuration
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["SecretKey"]).Returns("your-256-bit-secret-key-here-for-testing-purposes");
            jwtSection.Setup(x => x["Issuer"]).Returns("test-issuer");
            jwtSection.Setup(x => x["Audience"]).Returns("test-audience");

            _mockConfig.Setup(x => x.GetSection("jwt")).Returns(jwtSection.Object);

            _tokenService = new TokenService(_mockConfig.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public void GenerateToken_ForUser_ShouldGenerateValidToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                UserRoleId = 1
            };

            var userRole = new UserRole
            {
                Id = 1,
                RoleName = "User"
            };

            var mockRepo = new Mock<IGenericRepository<UserRole>>();
            _mockUnitOfWork.Setup(uow => uow.Repository<UserRole>())
                .Returns(mockRepo.Object);

            mockRepo.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(userRole);

            // Act
            var token = _tokenService.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            Assert.Equal("test-issuer", jwtToken.Issuer);
            Assert.Equal("test-audience", jwtToken.Audiences.First());
            Assert.Equal("1", jwtToken.Claims.First(c => c.Type == "sub").Value);
            Assert.Equal("test@example.com", jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value);
            Assert.Equal("User", jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        public void GenerateToken_ForOwner_ShouldGenerateValidToken()
        {
            // Arrange
            var owner = new Owner
            {
                Id = 2,
                Email = "owner@example.com",
                UserRoleId = 2
            };

            var userRole = new UserRole
            {
                Id = 2,
                RoleName = "Owner"
            };

            var mockRepo = new Mock<IGenericRepository<UserRole>>();
            _mockUnitOfWork.Setup(uow => uow.Repository<UserRole>())
                .Returns(mockRepo.Object);

            mockRepo.Setup(repo => repo.GetByIdAsync(2))
                .ReturnsAsync(userRole);

            // Act
            var token = _tokenService.GenerateToken(owner);

            // Assert
            Assert.NotNull(token);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            Assert.Equal("test-issuer", jwtToken.Issuer);
            Assert.Equal("test-audience", jwtToken.Audiences.First());
            Assert.Equal("2", jwtToken.Claims.First(c => c.Type == "sub").Value);
            Assert.Equal("owner@example.com", jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value);
            Assert.Equal("Owner", jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        public void GenerateToken_ForAdmin_ShouldGenerateValidToken()
        {
            // Arrange
            var admin = new Admin
            {
                Id = 3,
                Email = "admin@example.com",
                UserRoleId = 3
            };

            var userRole = new UserRole
            {
                Id = 3,
                RoleName = "Admin"
            };

            var mockRepo = new Mock<IGenericRepository<UserRole>>();
            _mockUnitOfWork.Setup(uow => uow.Repository<UserRole>())
                .Returns(mockRepo.Object);

            mockRepo.Setup(repo => repo.GetByIdAsync(3))
                .ReturnsAsync(userRole);

            // Act
            var token = _tokenService.GenerateToken(admin);

            // Assert
            Assert.NotNull(token);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            Assert.Equal("test-issuer", jwtToken.Issuer);
            Assert.Equal("test-audience", jwtToken.Audiences.First());
            Assert.Equal("3", jwtToken.Claims.First(c => c.Type == "sub").Value);
            Assert.Equal("admin@example.com", jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value);
            Assert.Equal("Admin", jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        public void GenerateToken_WithUnknownRole_ShouldUseUnknownRoleName()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                UserRoleId = 999 // Non-existent role
            };

            var mockRepo = new Mock<IGenericRepository<UserRole>>();
            _mockUnitOfWork.Setup(uow => uow.Repository<UserRole>())
                .Returns(mockRepo.Object);

            mockRepo.Setup(repo => repo.GetByIdAsync(999))
                .ReturnsAsync((UserRole?)null);

            // Act
            var token = _tokenService.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            Assert.Equal("Unknown", jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        public void Constructor_WithMissingSecretKey_ShouldThrowException()
        {
            // Arrange
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["SecretKey"]).Returns((string?)null);
            jwtSection.Setup(x => x["Issuer"]).Returns("test-issuer");
            jwtSection.Setup(x => x["Audience"]).Returns("test-audience");

            _mockConfig.Setup(x => x.GetSection("jwt")).Returns(jwtSection.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new TokenService(_mockConfig.Object, _mockUnitOfWork.Object));
        }
    }
} 