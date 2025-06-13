using backend.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Core.Interfaces;

public interface IBookingService
{
    Task<(bool Success, string Message)> BookCourtAsync(object request, int userId);
    Task<object> GetBookingsForCourtAsync(int courtId, DateOnly date);
    Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, int userId);
    Task<(bool Success, string Message)> ConfirmBookingAsync(int bookingId, int userId);
    Task<IEnumerable<object>> GetUserBookingsAsync(int userId, BookingStatus? status = null);
    Task<(bool Success, string Message)> CheckInBookingAsync(int bookingId, int ownerId);
    Task<IEnumerable<object>> GetBookingsForFacilityAsync(int facilityId, DateOnly date);
    Task HandleNoShowsAsync();
} 