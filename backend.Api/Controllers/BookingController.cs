using System.Security.Claims;
using backend.Api.DTOs.Booking;
using backend.Api.Services;
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
        var UserId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
        var success = await _bookingService.BookCourtAsync(request, UserId);
        
        if (!success)
            return BadRequest("Booking failed. The court may be unavailable or the time is invalid.");

        return Ok("Booking successful!");
    }

    [HttpGet("{courtId}/{date}")]
    public async Task<IActionResult> GetBookings(int courtId, DateOnly date)
    {
        var bookings = await _bookingService.GetBookingsForCourtAsync(courtId, date);
        return Ok(bookings);
    }
}
