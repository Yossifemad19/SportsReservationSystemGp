using AutoMapper;
using backend.Api.Services;
using backend.Api.Services.Interfaces;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;
using Moq;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace backend.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockMapper = new Mock<IMapper>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTokenService = new Mock<ITokenService>();
            _mockEmailService = new Mock<IEmailService>();
            _authService = new AuthService(_mockMapper.Object, _mockUnitOfWork.Object, _mockTokenService.Object, _mockEmailService.Object);
        }

        [Fact]
        public async Task Register_ValidData_ReturnsToken()
        {
            // Arrange
            var registerDto = new backend.Api.DTOs.RegisterDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@test.com",
                Password = "password123",
                PhoneNumber = "1234567890"
            };

            var user = new User
            {
                Id = 1,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                UserRoleId = 1
            };

            var userRole = new UserRole { Id = 1, RoleName = "Customer" };

            _mockMapper.Setup(x => x.Map<User>(registerDto))
                .Returns(user);

            _mockUnitOfWork.Setup(x => x.Repository<UserRole>())
                .Returns(Mock.Of<IGenericRepository<UserRole>>(r => 
                    r.GetFirstOrDefaultAsync(It.IsAny<UserRoleSpecification>()) == Task.FromResult(userRole)));

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("test-token");

            var mockUserRepo = new Mock<IGenericRepository<User>>();
            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(mockUserRepo.Object);
            
            var mockOwnerRepo = new Mock<IGenericRepository<Owner>>();
            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(mockOwnerRepo.Object);


            // Act
            var result = await _authService.Register(registerDto, "Customer");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("test-token", result.Data);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsUserResponseDto()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                UserRoleId = 1
            };

            var userRole = new UserRole { Id = 1, RoleName = "Customer" };

            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(Mock.Of<IGenericRepository<User>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()) == Task.FromResult(user)));

            _mockUnitOfWork.Setup(x => x.Repository<UserRole>())
                .Returns(Mock.Of<IGenericRepository<UserRole>>(r => 
                    r.GetByIdAsync(It.IsAny<int>()) == Task.FromResult(userRole)));

            _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("test-token");

            // Act
            var result = await _authService.Login(new backend.Api.DTOs.LoginDto
            {
                Email = "test@test.com",
                Password = "password123"
            });

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Test User", result.Data.Name);
            Assert.Equal("test@test.com", result.Data.Email);
            Assert.Equal("test-token", result.Data.Token);
            Assert.Equal("Customer", result.Data.Role);
        }

        [Fact]
        public async Task ForgotPassword_ValidEmail_SendsResetEmail()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@test.com",
                ResetToken = null,
                ResetTokenExpiry = null
            };

            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(Mock.Of<IGenericRepository<User>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()) == Task.FromResult(user)));

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.ForgotPassword("test@test.com");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Reset token generated.", result.Message);
            Assert.NotNull(user.ResetToken);
            Assert.NotNull(user.ResetTokenExpiry);
        }

        [Fact]
        public async Task ResetPassword_ValidToken_ResetsPassword()
        {
            // Arrange
            var resetToken = Guid.NewGuid().ToString();
            var user = new User
            {
                Id = 1,
                Email = "test@test.com",
                ResetToken = resetToken,
                ResetTokenExpiry = DateTime.UtcNow.AddHours(1)
            };

            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(Mock.Of<IGenericRepository<User>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()) == Task.FromResult(user)));

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _authService.ResetPassword(new backend.Api.DTOs.PasswordDto
            {
                Token = resetToken,
                NewPassword = "newpassword123"
            });

            // Assert
            Assert.Equal("Password has been reset successfully.", result.Data);
            Assert.Null(user.ResetToken);
            Assert.Null(user.ResetTokenExpiry);
            Assert.NotNull(user.PasswordHash);
        }

        [Fact]
        public async Task OwnerRegister_ValidData_ReturnsSuccessMessage()
        {
            // Arrange
            var ownerRegisterDto = new backend.Api.DTOs.OwnerRegisterDto
            {
                FirstName = "Test",
                LastName = "Owner",
                Email = "owner@test.com",
                Password = "password123",
                PhoneNumber = "1234567890"
            };

            var owner = new Owner
            {
                Id = 1,
                FirstName = ownerRegisterDto.FirstName,
                LastName = ownerRegisterDto.LastName,
                Email = ownerRegisterDto.Email,
                PhoneNumber = ownerRegisterDto.PhoneNumber,
                UserRoleId = 1,
                IsApproved = false
            };

            var userRole = new UserRole { Id = 1, RoleName = "Owner" };

            _mockMapper.Setup(x => x.Map<Owner>(ownerRegisterDto))
                .Returns(owner);

            _mockUnitOfWork.Setup(x => x.Repository<UserRole>())
                .Returns(Mock.Of<IGenericRepository<UserRole>>(r => 
                    r.GetFirstOrDefaultAsync(It.IsAny<UserRoleSpecification>()) == Task.FromResult(userRole)));
            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            var mockUserRepo = new Mock<IGenericRepository<User>>();
            _mockUnitOfWork.Setup(u=>u.Repository<User>()).Returns(mockUserRepo.Object);
            
            var mockOwnerRepo = new Mock<IGenericRepository<Owner>>();
            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(mockOwnerRepo.Object);

            // Act
            var result = await _authService.OwnerRegister(ownerRegisterDto, "Owner");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Registration successful. Please wait for admin approval.", result.Data);
        }

        [Fact]
        public async Task OwnerLogin_ValidCredentials_ReturnsOwnerResponseDto()
        {
            // Arrange
            var owner = new Owner
            {
                Id = 1,
                FirstName = "Test",
                LastName = "Owner",
                Email = "owner@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                UserRoleId = 1,
                IsApproved = true
            };

            var userRole = new UserRole { Id = 1, RoleName = "Owner" };

            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(Mock.Of<IGenericRepository<Owner>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Owner, bool>>>()) == Task.FromResult(owner)));

            _mockUnitOfWork.Setup(x => x.Repository<UserRole>())
                .Returns(Mock.Of<IGenericRepository<UserRole>>(r => 
                    r.GetByIdAsync(It.IsAny<int>()) == Task.FromResult(userRole)));

            _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<Owner>()))
                .Returns("test-token");

            // Act
            var result = await _authService.OwnerLogin(new backend.Api.DTOs.OwnerLoginDto
            {
                Email = "owner@test.com",
                Password = "password123"
            });

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Test Owner", result.Data.Name);
            Assert.Equal("owner@test.com", result.Data.Email);
            Assert.Equal("test-token", result.Data.Token);
            Assert.Equal("Owner", result.Data.Role);
            Assert.Equal("True", result.Data.IsApproved);
        }

        [Fact]
        public async Task OwnerLogin_InvalidCredentials_ReturnsNull()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(Mock.Of<IGenericRepository<Owner>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Owner, bool>>>()) == Task.FromResult<Owner>(null!)));

            // Act
            var result = await _authService.OwnerLogin(new backend.Api.DTOs.OwnerLoginDto
            {
                Email = "wrong@test.com",
                Password = "wrongpassword"
            });

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateUserProfile_ValidData_ReturnsUpdatedProfile()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "Old",
                LastName = "Name",
                Email = "old@test.com",
                PhoneNumber = "1234567890"
            };

            var userProfileDto = new backend.Api.DTOs.UserProfileDto
            {
                FirstName = "New",
                LastName = "Name",
                Email = "new@test.com",
                PhoneNumber = "0987654321"
            };

            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(Mock.Of<IGenericRepository<User>>(r => 
                    r.GetByIdAsync(It.IsAny<int>()) == Task.FromResult(user)));

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _authService.UpdateUserProfile(1, userProfileDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("New", result.Data.FirstName);
            Assert.Equal("Name", result.Data.LastName);
            Assert.Equal("new@test.com", result.Data.Email);
            Assert.Equal("0987654321", result.Data.PhoneNumber);
        }

        [Fact]
        public async Task UpdateUserProfile_InvalidUserId_ReturnNotFound()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(Mock.Of<IGenericRepository<User>>(r => 
                    r.GetByIdAsync(It.IsAny<int>()) == Task.FromResult<User>(null!)));

            // Act & Assert
            var result = await _authService.UpdateUserProfile(999, new backend.Api.DTOs.UserProfileDto());
            Assert.False(result.Success);
            Assert.Equal($"User with id {999} not found.",result.Message);
        }

        [Fact]
        public async Task DeleteUser_ValidUserId_ReturnsTrue()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                Email = "test@test.com"
            };

            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(Mock.Of<IGenericRepository<User>>(r => 
                    r.GetByIdAsync(It.IsAny<int>()) == Task.FromResult(user)));

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _authService.DeleteUser(1);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteUser_InvalidUserId_RetrunNotFound()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(Mock.Of<IGenericRepository<User>>(r => 
                    r.GetByIdAsync(It.IsAny<int>()) == Task.FromResult<User>(null!)));

            // Act & Assert
            var result = await _authService.DeleteUser(999);
            Assert.False(result.Success);
            Assert.Equal($"User with id {999} not found.", result.Message);
        }

        [Fact]
        public async Task IsEmailExist_ExistingUserEmail_ReturnsTrue()
        {
            // Arrange
            var user = new User { Email = "test@test.com" };

            var mockUserRepo = new Mock<IGenericRepository<User>>();
            mockUserRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                .ReturnsAsync(user);
            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(mockUserRepo.Object);

            var mockOwnerRepo = new Mock<IGenericRepository<Owner>>();
            mockOwnerRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Owner, bool>>>()))
                .ReturnsAsync((Owner)null!);
            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(mockOwnerRepo.Object);

            // Act
            var result = await _authService.IsEmailExist("test@test.com");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEmailExist_ExistingOwnerEmail_ReturnsTrue()
        {
            // Arrange
            var owner = new Owner { Email = "owner@test.com" };

            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(Mock.Of<IGenericRepository<User>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()) == Task.FromResult<User>(null!)));

            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(Mock.Of<IGenericRepository<Owner>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Owner, bool>>>()) == Task.FromResult(owner)));

            // Act
            var result = await _authService.IsEmailExist("owner@test.com");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEmailExist_NonExistingEmail_ReturnsFalse()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Repository<User>())
                .Returns(Mock.Of<IGenericRepository<User>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()) == Task.FromResult<User>(null!)));

            _mockUnitOfWork.Setup(x => x.Repository<Owner>())
                .Returns(Mock.Of<IGenericRepository<Owner>>(r => 
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Owner, bool>>>()) == Task.FromResult<Owner>(null!)));

            // Act
            var result = await _authService.IsEmailExist("nonexistent@test.com");

            // Assert
            Assert.False(result);
        }
    }
} 