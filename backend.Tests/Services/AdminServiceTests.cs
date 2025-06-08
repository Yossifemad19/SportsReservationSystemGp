using backend.Api.Services;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;
using Moq;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace backend.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly AdminService _adminService;

        public AdminServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTokenService = new Mock<ITokenService>();
            _adminService = new AdminService(_mockUnitOfWork.Object, _mockTokenService.Object);
        }

        [Fact]
        public async Task ApproveOwner_ValidOwnerId_ReturnsTrue()
        {
            // Arrange
            var ownerRole = new UserRole { Id = 1, RoleName = "Owner" };
            var owner = new Owner { Id = 1, UserRoleId = ownerRole.Id, IsApproved = false };

            _mockUnitOfWork.Setup(x => x.Repository<UserRole>())
                .Returns(Mock.Of<IGenericRepository<UserRole>>(r => 
                    r.GetFirstOrDefaultAsync(It.IsAny<UserRoleSpecification>()) == Task.FromResult(ownerRole)));

            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(Mock.Of<IGenericRepository<Owner>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Owner, bool>>>()) == Task.FromResult(owner)));

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _adminService.ApproveOwner(1);

            // Assert
            Assert.True(result.Success);
            Assert.True(owner.IsApproved);
        }

        [Fact]
        public async Task ApproveOwner_InvalidOwnerId_ReturnsFalse()
        {
            // Arrange
            var ownerRole = new UserRole { Id = 1, RoleName = "Owner" };

            _mockUnitOfWork.Setup(x => x.Repository<UserRole>())
                .Returns(Mock.Of<IGenericRepository<UserRole>>(r => 
                    r.GetFirstOrDefaultAsync(It.IsAny<UserRoleSpecification>()) == Task.FromResult(ownerRole)));

            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(Mock.Of<IGenericRepository<Owner>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Owner, bool>>>()) == Task.FromResult<Owner>(null)));

            // Act
            var result = await _adminService.ApproveOwner(999);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task AdminLogin_ValidCredentials_ReturnsUserResponseDto()
        {
            // Arrange
            var admin = new Admin
            {
                Id = 1,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                UserRoleId = 1
            };

            var adminRole = new UserRole { Id = 1, RoleName = "Admin" };

            _mockUnitOfWork.Setup(x => x.Repository<Admin>())
                .Returns(Mock.Of<IGenericRepository<Admin>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Admin, bool>>>()) == Task.FromResult(admin)));

            _mockUnitOfWork.Setup(x => x.Repository<UserRole>())
                .Returns(Mock.Of<IGenericRepository<UserRole>>(r => 
                    r.GetByIdAsync(It.IsAny<int>()) == Task.FromResult(adminRole)));

            _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<Admin>()))
                .Returns("test-token");

            // Act
            var result = await _adminService.AdminLogin(new backend.Api.DTOs.AdminLoginDto
            {
                Email = "admin@test.com",
                Password = "password123"
            });

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Admin User", result.Data.Name);
            Assert.Equal("admin@test.com", result.Data.Email);
            Assert.Equal("test-token", result.Data.Token);
            Assert.Equal("Admin", result.Data.Role);
        }

        [Fact]
        public async Task AdminLogin_InvalidCredentials_ReturnsNull()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Repository<Admin>())
                .Returns(Mock.Of<IGenericRepository<Admin>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Admin, bool>>>()) == Task.FromResult<Admin>(null)));

            // Act
            var result = await _adminService.AdminLogin(new backend.Api.DTOs.AdminLoginDto
            {
                Email = "wrong@test.com",
                Password = "wrongpassword"
            });

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid email or password.", result.Message);
        }

        [Fact]
        public async Task RejectOwner_ValidOwnerId_ReturnsTrue()
        {
            // Arrange
            var owner = new Owner { Id = 1, FirstName = "Test", LastName = "Owner" };

            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(Mock.Of<IGenericRepository<Owner>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Owner, bool>>>()) == Task.FromResult(owner)));

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _adminService.RejectOwner(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Owner rejected and deleted.",result.Message);
        }

        [Fact]
        public async Task RejectOwner_InvalidOwnerId_ReturnsFalse()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(Mock.Of<IGenericRepository<Owner>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Owner, bool>>>()) == Task.FromResult<Owner>(null)));

            // Act
            var result = await _adminService.RejectOwner(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Owner not found.",result.Message);
        }

        [Fact]
        public async Task GetAllOwners_ReturnsListOfOwners()
        {
            // Arrange
            var owners = new List<Owner>
            {
                new Owner { Id = 1, FirstName = "Owner1", LastName = "Test", Email = "owner1@test.com" },
                new Owner { Id = 2, FirstName = "Owner2", LastName = "Test", Email = "owner2@test.com" }
            };

            var mockOwnerRepo = new Mock<IGenericRepository<Owner>>();
            mockOwnerRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(owners);

            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(mockOwnerRepo.Object);

            // Act
            var result = await _adminService.GetAllOwners();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count());
            Assert.Contains(result.Data, o => o.Email == "owner1@test.com");
            Assert.Contains(result.Data, o => o.Email == "owner2@test.com");
        }

        [Fact]
        public async Task GetAllUsers_ReturnsListOfUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, FirstName = "User1", LastName = "Test", Email = "user1@test.com" },
                new User { Id = 2, FirstName = "User2", LastName = "Test", Email = "user2@test.com" }
            };

            var mockUserRepo = new Mock<IGenericRepository<User>>();
            mockUserRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(users);

            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(mockUserRepo.Object);

            // Act
            var result = await _adminService.GetAllUsers();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count());
            Assert.Contains(result.Data, u => u.Email == "user1@test.com");
            Assert.Contains(result.Data, u => u.Email == "user2@test.com");
        }

        [Fact]
        public async Task GetAllUnApprovedOwners_ReturnsOnlyUnapprovedOwners()
        {
            // Arrange
            var owners = new List<Owner>
            {
                new Owner { Id = 1, FirstName = "Owner1", LastName = "Test", Email = "owner1@test.com", IsApproved = false },
                new Owner { Id = 2, FirstName = "Owner2", LastName = "Test", Email = "owner2@test.com", IsApproved = true }
            };

            var mockOwnerRepo = new Mock<IGenericRepository<Owner>>();
            mockOwnerRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(owners);

            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(mockOwnerRepo.Object);

            // Act
            var result = await _adminService.GetAllUnApprovedOwners();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data);
            Assert.Contains(result.Data, o => o.Email == "owner1@test.com");
            Assert.DoesNotContain(result.Data, o => o.Email == "owner2@test.com");
        }

        [Fact]
        public async Task GetOwnerById_ValidId_ReturnsOwner()
        {
            // Arrange
            var owner = new Owner
            {
                Id = 1,
                FirstName = "Test",
                LastName = "Owner",
                Email = "owner@test.com"
            };

            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(Mock.Of<IGenericRepository<Owner>>(r => 
                    r.GetByIdAsync(It.IsAny<int>()) == Task.FromResult(owner)));

            // Act
            var result = await _adminService.GetOwnerById(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Test", result.Data.FirstName);
            Assert.Equal("Owner", result.Data.LastName);
            Assert.Equal("owner@test.com", result.Data.Email);
        }

        [Fact]
        public async Task GetOwnerById_InvalidId_returnSuccessFalse()
        {
            // handle the exception 
            // Arrange
            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(Mock.Of<IGenericRepository<Owner>>(r => 
                    r.GetByIdAsync(It.IsAny<int>()) == Task.FromResult<Owner>(null!)));

            // Act & Assert
            var result = await _adminService.GetOwnerById(999);
            Assert.False(result.Success);
            Assert.Equal($"Owner with id {999} not found.",result.Message);
        }

        [Fact]
        public async Task GetUserById_ValidId_ReturnsUser()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                Email = "user@test.com"
            };

            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(Mock.Of<IGenericRepository<User>>(r => 
                    r.GetByIdAsync(It.IsAny<int>()) == Task.FromResult(user)));

            // Act
            var result = await _adminService.GetUserById(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Test", result.Data.FirstName);
            Assert.Equal("User", result.Data.LastName);
            Assert.Equal("user@test.com", result.Data.Email);
        }

        [Fact]
        public async Task GetUserById_InvalidId_ReturnSucessFalse()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(Mock.Of<IGenericRepository<User>>(r => 
                    r.GetByIdAsync(It.IsAny<int>()) == Task.FromResult<User>(null!)));

            // Act & Assert
            var result = await _adminService.GetUserById(999);
            Assert.False(result.Success);
            Assert.Equal($"User with id {999} not found.",result.Message);
        }
    }
} 