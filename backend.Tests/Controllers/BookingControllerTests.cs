using System.Security.Claims;
using backend.Api.Controllers;
using backend.Api.DTOs;
using backend.Api.DTOs.Booking;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace backend.Tests.Controllers;

public class BookingControllerTests
{
    private readonly Mock<IBookingService> _mockBookingService;
    private readonly BookingController _controller;

    public BookingControllerTests()
    {
        _mockBookingService = new Mock<IBookingService>();
        _controller = new BookingController(_mockBookingService.Object);
    }

    [Fact]
    public async Task BookCourt_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var request = new BookingRequestDto
        {
            CourtId = 1,
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = TimeSpan.FromHours(10),
            EndTime = TimeSpan.FromHours(11)
        };

        var userId = 1;
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim(ClaimTypes.Role, "Customer")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockBookingService.Setup(service => service.BookCourtAsync(request, userId))
            .ReturnsAsync((true, "Booking created successfully"));

        // Act
        var result = await _controller.BookCourt(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Booking created successfully", okResult.Value);
    }

    [Fact]
    public async Task BookCourt_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new BookingRequestDto
        {
            CourtId = 1,
            Date = DateOnly.FromDateTime(DateTime.Today),
            StartTime = TimeSpan.FromHours(10),
            EndTime = TimeSpan.FromHours(11)
        };

        var userId = 1;
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim(ClaimTypes.Role, "Customer")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockBookingService.Setup(service => service.BookCourtAsync(request, userId))
            .ReturnsAsync((false, "Court is already booked for this time slot"));

        // Act
        var result = await _controller.BookCourt(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Court is already booked for this time slot", badRequestResult.Value);
    }

    [Fact]
    public async Task GetBookings_ShouldReturnBookings()
    {
        // Arrange
        var courtId = 1;
        var date = DateOnly.FromDateTime(DateTime.Today);
        var expectedBookings = new BookingResponseDto
        {
            CourtId = courtId,
            StartDate = date,
            BookingSlots = new Dictionary<string, List<SlotBlockDto>>
            {
                {
                    date.ToString("yyyy-MM-dd"),
                    new List<SlotBlockDto>
                    {
                        new() { StartTime = TimeSpan.FromHours(10).ToString(), EndTime = TimeSpan.FromHours(11).ToString() },
                        new() { StartTime = TimeSpan.FromHours(14).ToString(), EndTime = TimeSpan.FromHours(15).ToString() }
                    }
                }
            }
        };

        _mockBookingService.Setup(service => service.GetBookingsForCourtAsync(courtId, date))
            .ReturnsAsync(expectedBookings);

        // Act
        var result = await _controller.GetBookings(courtId, date);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<BookingResponseDto>(okResult.Value);
        Assert.Equal(courtId, returnValue.CourtId);
        Assert.Equal(date, returnValue.StartDate);
        Assert.Single(returnValue.BookingSlots);
    }

    [Fact]
    public async Task CancelBooking_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        var bookingId = 1;
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim(ClaimTypes.Role, "Customer")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockBookingService.Setup(service => service.CancelBookingAsync(bookingId, userId))
            .ReturnsAsync((true, "Booking cancelled successfully"));

        // Act
        var result = await _controller.CancelBooking(bookingId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Booking cancelled successfully", okResult.Value);
    }

    [Fact]
    public async Task ConfirmBooking_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var userId = 1;
        var bookingId = 1;
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim(ClaimTypes.Role, "Customer")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockBookingService.Setup(service => service.ConfirmBookingAsync(bookingId, userId))
            .ReturnsAsync((true, "Booking confirmed successfully"));

        // Act
        var result = await _controller.ConfirmBooking(bookingId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Booking confirmed successfully", okResult.Value);
    }

    [Fact]
    public async Task GetUserBookings_WithNoStatus_ShouldReturnAllBookings()
    {
        // Arrange
        var userId = 1;
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim(ClaimTypes.Role, "Customer")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var expectedBookings = new List<BookingDto>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                UserName = "John_Doe",
                CourtId = 1,
                CourtName = "Court 1",
                FacilityName = "Facility 1",
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11),
                Status = BookingStatus.Pending.ToString(),
                TotalPrice = 100
            },
            new()
            {
                Id = 2,
                UserId = userId,
                UserName = "John_Doe",
                CourtId = 2,
                CourtName = "Court 2",
                FacilityName = "Facility 2",
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeSpan.FromHours(14),
                EndTime = TimeSpan.FromHours(15),
                Status = BookingStatus.Confirmed.ToString(),
                TotalPrice = 100
            }
        };

        _mockBookingService.Setup(service => service.GetUserBookingsAsync(userId, null))
            .ReturnsAsync(expectedBookings.Cast<object>());

        // Act
        var result = await _controller.GetUserBookings();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        var bookingsList = returnValue.Cast<BookingDto>().ToList();
        Assert.Equal(2, bookingsList.Count);
    }

    [Fact]
    public async Task GetUserBookings_WithStatus_ShouldReturnFilteredBookings()
    {
        // Arrange
        var userId = 1;
        var status = "Confirmed";
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim(ClaimTypes.Role, "Customer")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var expectedBookings = new List<BookingDto>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                UserName = "John_Doe",
                CourtId = 1,
                CourtName = "Court 1",
                FacilityName = "Facility 1",
                Date = DateOnly.FromDateTime(DateTime.Today),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11),
                Status = BookingStatus.Confirmed.ToString(),
                TotalPrice = 100
            }
        };

        _mockBookingService.Setup(service => service.GetUserBookingsAsync(userId, BookingStatus.Confirmed))
            .ReturnsAsync(expectedBookings.Cast<object>());

        // Act
        var result = await _controller.GetUserBookings(status);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        var bookingsList = returnValue.Cast<BookingDto>().ToList();
        Assert.Single(bookingsList);
        Assert.Equal(BookingStatus.Confirmed.ToString(), bookingsList[0].Status);
    }

    [Fact]
    public async Task CheckInBooking_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var ownerId = 1;
        var bookingId = 1;
        var claims = new List<Claim>
        {
            new Claim("sub", ownerId.ToString()),
            new Claim(ClaimTypes.Role, "Owner")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockBookingService.Setup(service => service.CheckInBookingAsync(bookingId, ownerId))
            .ReturnsAsync((true, "User successfully checked in"));

        // Act
        var result = await _controller.CheckInBooking(bookingId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("User successfully checked in", okResult.Value);
    }

    [Fact]
    public async Task GetFacilityBookings_WithDate_ShouldReturnBookings()
    {
        // Arrange
        var ownerId = 1;
        var facilityId = 1;
        var date = DateOnly.FromDateTime(DateTime.Today);
        var claims = new List<Claim>
        {
            new Claim("sub", ownerId.ToString()),
            new Claim(ClaimTypes.Role, "Owner")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var expectedBookings = new List<BookingDto>
        {
            new()
            {
                Id = 1,
                UserId = 1,
                UserName = "John_Doe",
                CourtId = 1,
                CourtName = "Court 1",
                FacilityName = "Facility 1",
                Date = date,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11),
                Status = BookingStatus.Confirmed.ToString(),
                TotalPrice = 100
            }
        };

        _mockBookingService.Setup(service => service.GetBookingsForFacilityAsync(facilityId, date))
            .ReturnsAsync(expectedBookings.Cast<object>());

        // Act
        var result = await _controller.GetFacilityBookings(facilityId, date);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        var bookingsList = returnValue.Cast<BookingDto>().ToList();
        Assert.Single(bookingsList);
    }

    [Fact]
    public async Task GetFacilityBookings_WithoutDate_ShouldReturnTodayBookings()
    {
        // Arrange
        var ownerId = 1;
        var facilityId = 1;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var claims = new List<Claim>
        {
            new Claim("sub", ownerId.ToString()),
            new Claim(ClaimTypes.Role, "Owner")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var expectedBookings = new List<BookingDto>
        {
            new()
            {
                Id = 1,
                UserId = 1,
                UserName = "John_Doe",
                CourtId = 1,
                CourtName = "Court 1",
                FacilityName = "Facility 1",
                Date = today,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11),
                Status = BookingStatus.Confirmed.ToString(),
                TotalPrice = 100
            }
        };

        _mockBookingService.Setup(service => service.GetBookingsForFacilityAsync(facilityId, today))
            .ReturnsAsync(expectedBookings.Cast<object>());

        // Act
        var result = await _controller.GetFacilityBookings(facilityId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        var bookingsList = returnValue.Cast<BookingDto>().ToList();
        Assert.Single(bookingsList);
    }
} 