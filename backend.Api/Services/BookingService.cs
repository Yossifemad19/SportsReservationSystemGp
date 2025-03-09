using backend.Api.DTOs.Booking;
using backend.Core.Entities;
using backend.Core.Interfaces;

namespace backend.Api.Services;

public class BookingService: IBookingService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> BookCourtAsync(BookingRequestDto request, int userId)
    {
        // check if starttime greater than or equal endTime
        if (request.StartTime >= request.EndTime)
            return false;
        // Check if the court exists
        var court = await _unitOfWork.Repository<Court>().GetByIdAsync(request.CourtId);
        if (court == null) return false;

        // Check facility opening hours
        var facility = await _unitOfWork.Repository<Facility>().GetByIdAsync(court.FacilityId);
        if (facility == null) return false;

        if (request.StartTime < court.OpeningTime || request.EndTime > court.ClosingTime)
            return false; // Booking outside facility hours

        // Check if the court is available
        bool isAvailable = await _unitOfWork.BookingRepository.IsCourtAvailableAsync(request.CourtId, request.Date, request.StartTime, request.EndTime);
        if (!isAvailable) return false;

        // Create booking
        var booking = new Booking
        {
            CourtId = request.CourtId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            UserId = userId
        };

        _unitOfWork.Repository<Booking>().Add(booking);
        await _unitOfWork.CompleteAsync(); // Save changes
        return true;
    }

    public async Task<BookingResponseDto> GetBookingsForCourtAsync(int courtId, DateOnly date)
    {
        // get all booking times include courts
        var bookings=await _unitOfWork.BookingRepository.GetBookingsForCourtAsync(courtId, date);
        // sslect specific fields for  booking slot field exist in SlotBlockDto and then do groupBy with time date 
        var bookingSlots = bookings
            .GroupBy(b => b.Date)
            .OrderBy(group => group.Key) 
            .ToDictionary(
                group => group.Key.ToString("yyyy-MM-dd"), 
                group => group.OrderBy(b => b.StartTime) 
                    .Select(b => new SlotBlockDto 
                    { 
                        StartTime = b.StartTime, 
                        EndTime = b.EndTime 
                    })
                    .ToList()
            );
        return new BookingResponseDto()
        {
            CourtId = courtId,
            StartDate = date,
            OpeningTime = bookings.Select(b=>b.Court.OpeningTime).FirstOrDefault(),
            ClosingTime = bookings.Select(b => b.Court.ClosingTime).FirstOrDefault(),
            BookingSlots = bookingSlots
        };
    }
}