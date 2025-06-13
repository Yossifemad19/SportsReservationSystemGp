using backend.Api.Services;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.Services
{
    public class BookingServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IGenericRepository<Court>> _mockCourtRepository;
        private readonly Mock<IGenericRepository<Booking>> _mockBookingRepository;
        private readonly Mock<IBookingRepository> _mockBookingRepositoryCustom;
        private readonly Mock<IGenericRepository<Facility>> _mockFacilityRepository;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCourtRepository = new Mock<IGenericRepository<Court>>();
            _mockBookingRepository = new Mock<IGenericRepository<Booking>>();
            _mockBookingRepositoryCustom = new Mock<IBookingRepository>();
            _mockFacilityRepository = new Mock<IGenericRepository<Facility>>();

            _mockUnitOfWork.Setup(uow => uow.Repository<Court>()).Returns(_mockCourtRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.Repository<Booking>()).Returns(_mockBookingRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.BookingRepository).Returns(_mockBookingRepositoryCustom.Object);
            _mockUnitOfWork.Setup(uow => uow.Repository<Facility>()).Returns(_mockFacilityRepository.Object);

            // Setup transaction mock
            var mockTransaction = new Mock<IDbContextTransaction>();
            _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync())
                .ReturnsAsync(mockTransaction.Object);

            _bookingService = new BookingService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task BookCourtAsync_WithValidRequest_ShouldSucceed()
        {
            // Arrange
            var request = new BookingRequestDto
            {
                CourtId = 1,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11)
            };

            var court = new Court
            {
                Id = 1,
                Facility = new Facility
                {
                    OpeningTime = TimeSpan.FromHours(8),
                    ClosingTime = TimeSpan.FromHours(22)
                }
            };

            _mockCourtRepository.Setup(repo => repo.GetByIdWithSpecAsync(It.IsAny<CourtWithFacilitySpecification>()))
                .ReturnsAsync(court);

            _mockBookingRepositoryCustom.Setup(repo => repo.IsCourtAvailableAsync(
                It.IsAny<int>(),
                It.IsAny<DateOnly>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()))
                .ReturnsAsync(true);

            _mockUnitOfWork.Setup(uow => uow.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _bookingService.BookCourtAsync(request, 1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Booking successful", result.Message);
            _mockBookingRepository.Verify(repo => repo.Add(It.IsAny<Booking>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.Complete(), Times.Once);
        }

        [Fact]
        public async Task BookCourtAsync_WithInvalidTimeRange_ShouldFail()
        {
            // Arrange
            var request = new BookingRequestDto
            {
                CourtId = 1,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                StartTime = TimeSpan.FromHours(11),
                EndTime = TimeSpan.FromHours(10)
            };

            // Act
            var result = await _bookingService.BookCourtAsync(request, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Start time must be before end time", result.Message);
        }

        [Fact]
        public async Task BookCourtAsync_WithNonExistentCourt_ShouldFail()
        {
            // Arrange
            var request = new BookingRequestDto
            {
                CourtId = 1,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11)
            };

            _mockCourtRepository.Setup(repo => repo.GetByIdWithSpecAsync(It.IsAny<CourtWithFacilitySpecification>()))
                .ReturnsAsync((Court)null);

            // Act
            var result = await _bookingService.BookCourtAsync(request, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Court not found", result.Message);
        }

        [Fact]
        public async Task BookCourtAsync_OutsideFacilityHours_ShouldFail()
        {
            // Arrange
            var request = new BookingRequestDto
            {
                CourtId = 1,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                StartTime = TimeSpan.FromHours(7),
                EndTime = TimeSpan.FromHours(8)
            };

            var court = new Court
            {
                Id = 1,
                Facility = new Facility
                {
                    OpeningTime = TimeSpan.FromHours(8),
                    ClosingTime = TimeSpan.FromHours(22)
                }
            };

            _mockCourtRepository.Setup(repo => repo.GetByIdWithSpecAsync(It.IsAny<CourtWithFacilitySpecification>()))
                .ReturnsAsync(court);

            // Act
            var result = await _bookingService.BookCourtAsync(request, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Booking outside facility hours", result.Message);
        }

        [Fact]
        public async Task BookCourtAsync_WhenCourtNotAvailable_ShouldFail()
        {
            // Arrange
            var request = new BookingRequestDto
            {
                CourtId = 1,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11)
            };

            var court = new Court
            {
                Id = 1,
                Facility = new Facility
                {
                    OpeningTime = TimeSpan.FromHours(8),
                    ClosingTime = TimeSpan.FromHours(22)
                }
            };

            _mockCourtRepository.Setup(repo => repo.GetByIdWithSpecAsync(It.IsAny<CourtWithFacilitySpecification>()))
                .ReturnsAsync(court);

            _mockBookingRepositoryCustom.Setup(repo => repo.IsCourtAvailableAsync(
                It.IsAny<int>(),
                It.IsAny<DateOnly>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()))
                .ReturnsAsync(false);

            // Act
            var result = await _bookingService.BookCourtAsync(request, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Court is not available at the requested time", result.Message);
        }

        [Fact]
        public async Task CancelBookingAsync_WithValidRequest_ShouldSucceed()
        {
            // Arrange
            var booking = new Booking
            {
                Id = 1,
                UserId = 1,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11),
                status = BookingStatus.Pending
            };

            _mockBookingRepositoryCustom.Setup(repo => repo.GetBookingWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(booking);

            _mockUnitOfWork.Setup(uow => uow.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _bookingService.CancelBookingAsync(1, 1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Booking cancelled successfully", result.Message);
            Assert.Equal(BookingStatus.Cancelled, booking.status);
            _mockUnitOfWork.Verify(uow => uow.Complete(), Times.Once);
        }

        [Fact]
        public async Task CancelBookingAsync_WithNonExistentBooking_ShouldFail()
        {
            // Arrange
            _mockBookingRepositoryCustom.Setup(repo => repo.GetBookingWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((Booking)null);

            // Act
            var result = await _bookingService.CancelBookingAsync(1, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Booking not found", result.Message);
        }

        [Fact]
        public async Task CancelBookingAsync_WithUnauthorizedUser_ShouldFail()
        {
            // Arrange
            var booking = new Booking
            {
                Id = 1,
                UserId = 2, // Different from the requesting user
                status = BookingStatus.Pending
            };

            _mockBookingRepositoryCustom.Setup(repo => repo.GetBookingWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(booking);

            // Act
            var result = await _bookingService.CancelBookingAsync(1, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You are not authorized to cancel this booking", result.Message);
        }

        [Fact]
        public async Task ConfirmBookingAsync_WithValidRequest_ShouldSucceed()
        {
            // Arrange
            var booking = new Booking
            {
                Id = 1,
                UserId = 1,
                status = BookingStatus.Pending
            };

            _mockBookingRepositoryCustom.Setup(repo => repo.GetBookingWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(booking);

            _mockUnitOfWork.Setup(uow => uow.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _bookingService.ConfirmBookingAsync(1, 1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Booking confirmed successfully", result.Message);
            Assert.Equal(BookingStatus.Confirmed, booking.status);
            _mockUnitOfWork.Verify(uow => uow.Complete(), Times.Once);
        }

        [Fact]
        public async Task GetUserBookingsAsync_ShouldReturnUserBookings()
        {
            // Arrange
            var bookings = new List<Booking>
            {
                new Booking
                {
                    Id = 1,
                    UserId = 1,
                    Court = new Court
                    {
                        Id = 1,
                        Name = "Court 1",
                        Facility = new Facility
                        {
                            Name = "Facility 1",
                            Address = new Address { City = "City 1" }
                        },
                        PricePerHour = 100
                    },
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    StartTime = TimeSpan.FromHours(10),
                    EndTime = TimeSpan.FromHours(11),
                    status = BookingStatus.Confirmed,
                    User = new User
                    {
                        FirstName = "John",
                        LastName = "Doe"
                    }
                }
            };

            _mockBookingRepositoryCustom.Setup(repo => repo.GetUserBookingsAsync(It.IsAny<int>(), It.IsAny<BookingStatus?>()))
                .ReturnsAsync(bookings);

            // Act
            var result = await _bookingService.GetUserBookingsAsync(1);

            // Assert
            Assert.Single(result);
            var bookingDto = result.First();
            Assert.Equal(1, bookingDto.Id);
            Assert.Equal(1, bookingDto.UserId);
            Assert.Equal("John_Doe", bookingDto.UserName);
            Assert.Equal("Court 1", bookingDto.CourtName);
            Assert.Equal("Facility 1", bookingDto.FacilityName);
            Assert.Equal("City 1", bookingDto.City);
            Assert.Equal(100, bookingDto.TotalPrice);
        }

        [Fact]
        public async Task CheckInBookingAsync_WithValidRequest_ShouldSucceed()
        {
            // Arrange
            var now = DateTime.Now;
            var booking = new Booking
            {
                Id = 1,
                Court = new Court
                {
                    Facility = new Facility
                    {
                        Id = 1,
                        OwnerId = 1
                    }
                },
                Date = DateOnly.FromDateTime(now),
                StartTime = TimeSpan.FromHours(now.Hour).Add(TimeSpan.FromMinutes(now.Minute)),
                EndTime = TimeSpan.FromHours(now.Hour).Add(TimeSpan.FromMinutes(now.Minute + 60)),
                status = BookingStatus.Confirmed
            };

            _mockBookingRepositoryCustom.Setup(repo => repo.GetBookingWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(booking);

            _mockFacilityRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(booking.Court.Facility);

            _mockUnitOfWork.Setup(uow => uow.Complete())
                .ReturnsAsync(1);

            // Act
            var result = await _bookingService.CheckInBookingAsync(1, 1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User successfully checked in", result.Message);
            Assert.Equal(BookingStatus.Completed, booking.status);
            Assert.NotNull(booking.CheckInTime);
            _mockUnitOfWork.Verify(uow => uow.Complete(), Times.Once);
        }

        [Fact]
        public async Task CheckInBookingAsync_WithUnauthorizedOwner_ShouldFail()
        {
            // Arrange
            var booking = new Booking
            {
                Id = 1,
                Court = new Court
                {
                    Facility = new Facility
                    {
                        Id = 1,
                        OwnerId = 2 // Different from the requesting owner
                    }
                },
                status = BookingStatus.Confirmed
            };

            _mockBookingRepositoryCustom.Setup(repo => repo.GetBookingWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(booking);

            _mockFacilityRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(booking.Court.Facility);

            // Act
            var result = await _bookingService.CheckInBookingAsync(1, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You are not authorized to check in this booking", result.Message);
        }
    }
} 