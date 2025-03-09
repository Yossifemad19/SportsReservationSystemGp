using backend.Core.Entities;

namespace backend.Core.Interfaces;

public interface IBookingRepository:IGenericRepository<Booking>
{
    Task<bool> IsCourtAvailableAsync(int courtId, DateOnly date, TimeSpan startTime, TimeSpan endTime);
    Task<IEnumerable<Booking>> GetBookingsForCourtAsync(int courtId, DateOnly date);
}