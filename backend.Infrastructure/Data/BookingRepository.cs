using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Data;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> IsCourtAvailableAsync(int courtId, DateOnly date, TimeSpan startTime, TimeSpan endTime)
    {
        return !await _context.Bookings
            .Where(b => b.CourtId == courtId && 
                        b.Date == date && 
                        b.status != BookingStatus.Cancelled) // Don't count cancelled bookings
            .AnyAsync(b => (b.StartTime < endTime && b.EndTime > startTime)); // Overlapping time check
    }

    public async Task<IEnumerable<Booking>> GetBookingsForCourtAsync(int courtId, DateOnly date)
    {
        return await _context.Bookings
            .Where(b => b.CourtId == courtId && 
                       (b.Date >= date && b.Date <= date.AddDays(7)) &&
                        b.status != BookingStatus.Cancelled) // Only show active bookings
            .Include(b => b.Court)
            .Include(b => b.Court.Facility)
            .ToListAsync();
    }

    public async Task<Booking> GetBookingWithDetailsAsync(int bookingId)
    {
        return await _context.Bookings
            .Include(b => b.Court)
            .Include(b => b.Court.Facility)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public async Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId, BookingStatus? status = null)
    {
        // Start with base query
        IQueryable<Booking> query = _context.Bookings
            .Where(b => b.UserId == userId);

        // Apply status filter 
        if (status.HasValue)
        {
            query = query.Where(b => b.status == status.Value);
        }

        // Add includes and ordering
        return await query
            .Include(b => b.Court)
                .ThenInclude(c => c.Facility)
                    .ThenInclude(f => f.Address) 
            .Include(b => b.User)
            .OrderByDescending(b => b.Date)
            .ThenBy(b => b.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsForFacilityAsync(int facilityId, DateOnly date)
    {
        return await _context.Bookings
            .Include(b => b.Court)
            .ThenInclude(c => c.Facility)
            .Include(b => b.User)
            .Where(b => b.Court.FacilityId == facilityId && b.Date == date && b.status != BookingStatus.Cancelled)
            .OrderBy(b => b.Court.Name)
            .ThenBy(b => b.StartTime)
            .ToListAsync();
    }
}