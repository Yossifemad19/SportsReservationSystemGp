using System.Security.Claims;
using backend.Api.DTOs.Booking;
using backend.Api.Services;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<IActionResult> BookCourt([FromBody] BookingRequestDto request)
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value);
        ;
        var result = await _bookingService.BookCourtAsync(request, userId);
        
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }

    [HttpGet("{courtId}/{date}")]
    public async Task<IActionResult> GetBookings(int courtId, DateOnly date)
    {
        var bookings = await _bookingService.GetBookingsForCourtAsync(courtId, date);
        return Ok(bookings);
    }
    
    [Authorize(Roles = "Customer")]
    [HttpPut("cancel/{bookingId}")]
    public async Task<IActionResult> CancelBooking(int bookingId)
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value);
        var result = await _bookingService.CancelBookingAsync(bookingId, userId);
        
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }
    
    [Authorize(Roles = "Customer")]
    [HttpPut("confirm/{bookingId}")]
    public async Task<IActionResult> ConfirmBooking(int bookingId)
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value);
        var result = await _bookingService.ConfirmBookingAsync(bookingId, userId);
        
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }
    
    [Authorize(Roles = "Customer")]
    [HttpGet("user")]
    public async Task<IActionResult> GetUserBookings([FromQuery] string status = null)
    {
        var userId = int.Parse(User.FindFirst("sub")?.Value);
        
        BookingStatus? bookingStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<BookingStatus>(status, true, out var parsedStatus))
        {
            bookingStatus = parsedStatus;
        }
        
        var bookings = await _bookingService.GetUserBookingsAsync(userId, bookingStatus);
        return Ok(bookings);
    }
    
    [Authorize(Roles = "Owner")]
    [HttpPut("checkin/{bookingId}")]
    public async Task<IActionResult> CheckInBooking(int bookingId)
    {
        var ownerId = int.Parse(User.FindFirst("sub")?.Value);
        var result = await _bookingService.CheckInBookingAsync(bookingId, ownerId);
        
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }
    
    [Authorize(Roles = "Owner")]
    [HttpGet("facility/{facilityId}")]
    public async Task<IActionResult> GetFacilityBookings(int facilityId, [FromQuery] DateOnly? date = null)
    {
        // Use today's date if not provided
        var bookingDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        
        var bookings = await _bookingService.GetBookingsForFacilityAsync(facilityId, bookingDate);
        return Ok(bookings);
    }
}