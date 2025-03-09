using backend.Api.DTOs.Booking;
using backend.Core.Entities;

namespace backend.Api.Services;


public interface IBookingService
{
    Task<bool> BookCourtAsync(BookingRequestDto requestDto, int userId);
    Task<BookingResponseDto> GetBookingsForCourtAsync(int courtId, DateOnly date);
}

