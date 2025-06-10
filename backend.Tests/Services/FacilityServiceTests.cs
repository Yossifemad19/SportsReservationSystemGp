/*using AutoMapper;
using backend.Api.DTOs;
using backend.Api.Services;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Tests.Services
{
    public class FacilityServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private readonly FacilityService _facilityService;

        public FacilityServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            _mockWebHostEnvironment.Setup(x => x.WebRootPath).Returns(Path.GetTempPath());
            _facilityService = new FacilityService(_mockUnitOfWork.Object, _mockMapper.Object, _mockWebHostEnvironment.Object);
        }

        [Fact]
        public async Task GetFacilityById_ValidId_ReturnsFacilityDto()
        {
            // Arrange
            var facility = new Facility
            {
                Id = 1,
                Name = "Test Facility",
                Address = new Address
                {
                    StreetAddress = "Test Street",
                    City = "Cairo",
                    Latitude = 30.0m,
                    Longitude = 31.2m
                }
            };

            var facilityDto = new FacilityDto
            {
                Id = 1,
                Name = "Test Facility",
                Address = new AddressDto
                {
                    StreetAddress = "Test Street",
                    City = "Cairo",
                    Latitude = 30.0m,
                    Longitude = 31.2m
                }
            };

            _mockUnitOfWork.Setup(x => x.Repository<Facility>())
                .Returns(Mock.Of<IGenericRepository<Facility>>(r =>
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Facility, bool>>>(),
                              It.IsAny<System.Linq.Expressions.Expression<System.Func<Facility, object>>>()) == Task.FromResult(facility)));

            _mockMapper.Setup(x => x.Map<FacilityDto>(facility))
                .Returns(facilityDto);

            // Act
            var result = await _facilityService.GetFacilityById(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Test Facility", result.Data.Name);
            Assert.NotNull(result.Data.Address);
            Assert.Equal("Test Street", result.Data.Address.StreetAddress);
            Assert.Equal("Cairo", result.Data.Address.City);
        }

        [Fact]
        public async Task GetFacilityById_InvalidId_ReturnsNull()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Repository<Facility>())
                .Returns(Mock.Of<IGenericRepository<Facility>>(r =>
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Facility, bool>>>(),
                              It.IsAny<System.Linq.Expressions.Expression<System.Func<Facility, object>>>()) == Task.FromResult<Facility>(null)));

            // Act
            var result = await _facilityService.GetFacilityById(999);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateFacility_ValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var facilityDto = new FacilityDto
            {
                Name = "Test Facility",
                Address = new AddressDto
                {
                    StreetAddress = "Test Street",
                    City = "Cairo",
                    Latitude = 30.0m,
                    Longitude = 31.2m
                },
                Image = new Mock<IFormFile>().Object
            };

            var facility = new Facility
            {
                Id = 1,
                Name = "Test Facility",
                Address = new Address
                {
                    StreetAddress = "Test Street",
                    City = "Cairo",
                    Latitude = 30.0m,
                    Longitude = 31.2m
                }
            };

            var facilityDtoResponse = new FacilityDto
            {
                Id = 1,
                Name = "Test Facility",
                Address = new AddressDto
                {
                    StreetAddress = "Test Street",
                    City = "Cairo",
                    Latitude = 30.0m,
                    Longitude = 31.2m
                }
            };

            _mockMapper.Setup(x => x.Map<Facility>(facilityDto))
                .Returns(facility);

            _mockMapper.Setup(x => x.Map<FacilityDto>(facility))
                .Returns(facilityDtoResponse);

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            var mockFacilityRepo = new Mock<IGenericRepository<Facility>>();
            _mockUnitOfWork.Setup(x => x.Repository<Facility>())
                .Returns(mockFacilityRepo.Object);

            // Act
            var result = await _facilityService.CreateFacility(facilityDto, "1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Facility created successfully.", result.Message);
            Assert.NotNull(result.Data);
           // var responseData = (FacilityDto)result.Data;
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Test Facility", result.Data.Name);
        }

        [Fact]
        public async Task CreateFacility_InvalidLocation_ReturnsErrorResponse()
        {
            // Arrange
            var facilityDto = new FacilityDto
            {
                Name = "Test Facility",
                Address = new AddressDto
                {
                    StreetAddress = "Test Street",
                    City = "Invalid City",
                    Latitude = 40.0m, // Outside Cairo/Giza
                    Longitude = 40.0m
                }
            };

            // Act
            var result = await _facilityService.CreateFacility(facilityDto, "1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Facility location must be inside Cairo or Giza.", result.Message);
        }

        [Fact]
        public async Task UpdateFacility_ValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var facilityDto = new FacilityDto
            {
                Id = 1,
                Name = "Updated Facility",
                Address = new AddressDto
                {
                    StreetAddress = "Updated Street",
                    City = "Cairo",
                    Latitude = 30.0m,
                    Longitude = 31.2m
                }
            };

            var existingFacility = new Facility
            {
                Id = 1,
                Name = "Original Facility",
                OwnerId = 1,
                Address = new Address
                {
                    StreetAddress = "Original Street",
                    City = "Cairo",
                    Latitude = 30.0m,
                    Longitude = 31.2m
                }
            };

            var updatedFacilityDto = new FacilityDto
            {
                Id = 1,
                Name = "Updated Facility",
                Address = new AddressDto
                {
                    StreetAddress = "Updated Street",
                    City = "Cairo",
                    Latitude = 30.0m,
                    Longitude = 31.2m
                }
            };

            _mockUnitOfWork.Setup(x => x.Repository<Facility>())
                .Returns(Mock.Of<IGenericRepository<Facility>>(r =>
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Facility, bool>>>(),
                              It.IsAny<System.Linq.Expressions.Expression<System.Func<Facility, object>>>()) == Task.FromResult(existingFacility)));

            _mockMapper.Setup(x => x.Map<FacilityDto>(It.IsAny<Facility>()))
                .Returns(updatedFacilityDto);

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _facilityService.UpdateFacility(facilityDto, "1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Facility updated successfully.", result.Message);
            Assert.NotNull(result.Data);
            //var responseData = (FacilityDto)result.Data;
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Updated Facility", result.Data.Name);
        }

        [Fact]
        public async Task UpdateFacility_InvalidOwner_ReturnsErrorResponse()
        {
            // Arrange
            var facilityDto = new FacilityDto
            {
                Id = 1,
                Name = "Test Facility",
                Address = new AddressDto
                {
                    StreetAddress = "Test Street",
                    City = "Cairo",
                    Latitude = 30.0m,
                    Longitude = 31.2m
                }
            };

            var existingFacility = new Facility
            {
                Id = 1,
                Name = "Test Facility",
                OwnerId = 2, // Different owner
                Address = new Address
                {
                    StreetAddress = "Test Street",
                    City = "Cairo",
                    Latitude = 30.0m,
                    Longitude = 31.2m
                }
            };

            _mockUnitOfWork.Setup(x => x.Repository<Facility>())
                .Returns(Mock.Of<IGenericRepository<Facility>>(r =>
                    r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Facility, bool>>>(),
                              It.IsAny<System.Linq.Expressions.Expression<System.Func<Facility, object>>>()) == Task.FromResult(existingFacility)));

            // Act
            var result = await _facilityService.UpdateFacility(facilityDto, "1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You do not own this facility.", result.Message);
        }

        [Fact]
        public async Task DeleteFacility_ValidId_ReturnsTrue()
        {
            // Arrange
            var facility = new Facility
            {
                Id = 1,
                Name = "Test Facility"
            };

            _mockUnitOfWork.Setup(x => x.Repository<Facility>())
                .Returns(Mock.Of<IGenericRepository<Facility>>(r =>
                    r.GetByIdAsync(It.IsAny<int>()) == Task.FromResult(facility)));

            _mockUnitOfWork.Setup(x => x.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _facilityService.DeleteFacility(1);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteFacility_InvalidId_ReturnsFalse()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.Repository<Facility>())
                .Returns(Mock.Of<IGenericRepository<Facility>>(r =>
                    r.GetByIdAsync(It.IsAny<int>()) == Task.FromResult<Facility>(null)));

            // Act
            var result = await _facilityService.DeleteFacility(999);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllFacilities_ReturnsListOfFacilities()
        {
            // Arrange
            var facilities = new List<Facility>
            {
                new Facility
                {
                    Id = 1,
                    Name = "Facility 1",
                    Address = new Address
                    {
                        StreetAddress = "Street 1",
                        City = "Cairo",
                        Latitude = 30.0m,
                        Longitude = 31.2m
                    }
                },
                new Facility
                {
                    Id = 2,
                    Name = "Facility 2",
                    Address = new Address
                    {
                        StreetAddress = "Street 2",
                        City = "Giza",
                        Latitude = 29.9m,
                        Longitude = 31.1m
                    }
                }
            };

            var facilityDtos = facilities.Select(f => new FacilityDto
            {
                Id = f.Id,
                Name = f.Name,
                Address = new AddressDto
                {
                    StreetAddress = f.Address.StreetAddress,
                    City = f.Address.City,
                    Latitude = f.Address.Latitude,
                    Longitude = f.Address.Longitude
                }
            }).ToList();

            var mockFacilityRepo = new Mock<IGenericRepository<Facility>>();
            mockFacilityRepo.Setup(r => r.GetAllIncludingAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Facility, object>>>()))
                .ReturnsAsync(facilities);

            _mockUnitOfWork.Setup(x => x.Repository<Facility>())
                .Returns(mockFacilityRepo.Object);

            _mockMapper.Setup(x => x.Map<List<FacilityDto>>(facilities))
                .Returns(facilityDtos);

            // Act
            var result = await _facilityService.GetAllFacilities();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, f => f.Name == "Facility 1");
            Assert.Contains(result.Data, f => f.Name == "Facility 2");
        }
    }
} */