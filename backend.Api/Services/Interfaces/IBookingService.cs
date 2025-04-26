using backend.Api.DTOs.Booking;
using backend.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Core.Interfaces;

public interface IBookingService
{
    Task<(bool Success, string Message)> BookCourtAsync(BookingRequestDto request, int userId);
    Task<BookingResponseDto> GetBookingsForCourtAsync(int courtId, DateOnly date);
    Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, int userId);
    Task<(bool Success, string Message)> ConfirmBookingAsync(int bookingId, int userId);
    Task<IEnumerable<BookingDto>> GetUserBookingsAsync(int userId, BookingStatus? status = null);
    Task<(bool Success, string Message)> CheckInBookingAsync(int bookingId, int ownerId);
    Task<IEnumerable<BookingDto>> GetBookingsForFacilityAsync(int facilityId, DateOnly date);
}