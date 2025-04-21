using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Data;

public class BookingRepository:GenericRepository<Booking>, IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> IsCourtAvailableAsync(int courtId, DateOnly date, TimeSpan startTime, TimeSpan endTime)
    {
        return !await _context.Bookings.AnyAsync(b =>
            b.CourtId == courtId &&
            b.Date == date &&
            (
                (b.StartTime < endTime && b.EndTime > startTime)  // Overlapping time check
            )
        );
    }

    public async Task<IEnumerable<Booking>> GetBookingsForCourtAsync(int courtId, DateOnly date)
    {
        return await _context.Bookings
            .Where(b => b.CourtId == courtId && (b.Date == date || b.Date <= date.AddDays(7) ) )
            .Include(b=>b.Court)
            .Include(b=>b.Court.Facility)
            .ToListAsync();
    }
}