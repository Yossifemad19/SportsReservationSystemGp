using backend.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Core.Interfaces;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<bool> IsCourtAvailableAsync(int courtId, DateOnly date, TimeSpan startTime, TimeSpan endTime);
    Task<IEnumerable<Booking>> GetBookingsForCourtAsync(int courtId, DateOnly date);
    Task<Booking> GetBookingWithDetailsAsync(int bookingId);
    Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId, BookingStatus? status = null);
    Task<IEnumerable<Booking>> GetBookingsForFacilityAsync(int facilityId, DateOnly date);
    Task<IEnumerable<Booking>> GetNoShowBookingsAsync(DateOnly currentDate, TimeOnly currentTime);
}