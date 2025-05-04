using backend.Api.DTOs.Booking;
using backend.Core.Entities;
using backend.Core.Interfaces;
using backend.Core.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Api.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    
    public async Task<(bool Success, string Message)> BookCourtAsync(BookingRequestDto request, int userId)
    {
        // Check if starttime greater than or equal endTime
        if (request.StartTime >= request.EndTime)
            return (false, "Start time must be before end time");
        
        // Check if the court exists
        var spec = new CourtWithFacilitySpecification(request.CourtId);
        var court = await _unitOfWork.Repository<Court>().GetByIdWithSpecAsync(spec);
        if (court == null) return (false, "Court not found");

        // Check facility opening hours
        if (request.StartTime < court.Facility.OpeningTime || request.EndTime > court.Facility.ClosingTime)
            return (false, "Booking outside facility hours"); 

        // Check if the court is available
        bool isAvailable = await _unitOfWork.BookingRepository.IsCourtAvailableAsync(request.CourtId, request.Date, request.StartTime, request.EndTime);
        if (!isAvailable) return (false, "Court is not available at the requested time");

        // Create booking
        var booking = new Booking
        {
            CourtId = request.CourtId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            UserId = userId,
            status = BookingStatus.Pending // initial value
        };

        _unitOfWork.Repository<Booking>().Add(booking);
        
        // Save changes
        var result = await _unitOfWork.Complete();
        if (result <= 0) return (false, "Failed to save booking");
        
        return (true, "Booking successful");
    }

    public async Task<BookingResponseDto> GetBookingsForCourtAsync(int courtId, DateOnly date)
    {
        // Get bookings for court with specific date
        var bookings = await _unitOfWork.BookingRepository.GetBookingsForCourtAsync(courtId, date);
        
        // Select specific fields for booking 
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
        
        // Get facility opening/closing times 
        var facilityTimes = bookings.FirstOrDefault()?.Court?.Facility;
        
        return new BookingResponseDto()
        {
            CourtId = courtId,
            StartDate = date,
            OpeningTime = facilityTimes?.OpeningTime ?? TimeSpan.Zero,
            ClosingTime = facilityTimes?.ClosingTime ?? TimeSpan.Zero,
            BookingSlots = bookingSlots
        };
    }

    public async Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, int userId)
    {
        // Get booking with details 
        var booking = await _unitOfWork.BookingRepository.GetBookingWithDetailsAsync(bookingId);
        
        // Validate booking
        if (booking == null)
            return (false, "Booking not found");
            
        // Verify user 
        if (booking.UserId != userId)
            return (false, "You are not authorized to cancel this booking");
            
        // Verify booking can be cancelled (not completed or cancelled)
        if (booking.status == BookingStatus.Completed || booking.status == BookingStatus.Cancelled)
            return (false, $"Cannot cancel booking with status: {booking.status}");
            
        // Check cancellation time policy (can't cancel less than 24 hours before)
        var bookingTimeOnly = TimeOnly.FromTimeSpan(booking.StartTime);
        var bookingDateTime = booking.Date.ToDateTime(bookingTimeOnly);
        if (DateTime.Now.AddHours(24) > bookingDateTime)
            return (false, "Bookings must be cancelled at least 24 hours in advance");
            
        // Update status
        booking.status = BookingStatus.Cancelled;
        
        _unitOfWork.Repository<Booking>().Update(booking);
        
        var result = await _unitOfWork.Complete();
        if (result <= 0) return (false, "Failed to cancel booking");
        
        return (true, "Booking cancelled successfully");
    }

    public async Task<(bool Success, string Message)> ConfirmBookingAsync(int bookingId, int userId)
    {
        // Get booking with details 
        var booking = await _unitOfWork.BookingRepository.GetBookingWithDetailsAsync(bookingId);
        
        // Validate booking
        if (booking == null)
            return (false, "Booking not found");
            
        // Verify user
        if (booking.UserId != userId)
            return (false, "You are not authorized to confirm this booking");
            
        // Verify booking can be confirmed (must be pending)
        if (booking.status != BookingStatus.Pending)
            return (false, $"Cannot confirm booking with status: {booking.status}");
            
        // Update status
        booking.status = BookingStatus.Confirmed;
        
        _unitOfWork.Repository<Booking>().Update(booking);
        
        // Save changes
        var result = await _unitOfWork.Complete();
        if (result <= 0) return (false, "Failed to confirm booking");
        
        return (true, "Booking confirmed successfully");
    }
    
    public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(int userId, BookingStatus? status = null)
    {
        // Get user bookings 
        var bookings = await _unitOfWork.BookingRepository.GetUserBookingsAsync(userId, status);
        
        return bookings.Select(b => new BookingDto
        {
            Id = b.Id,
            UserId = b.UserId,
            UserName = b.User?.FirstName+"_"+b.User?.LastName,
            CourtId = b.CourtId,
            CourtName = b.Court.Name,
            FacilityName = b.Court.Facility.Name,
            Date = b.Date,
            StartTime = b.StartTime,
            EndTime = b.EndTime,
            Status = b.status.ToString(),
            TotalPrice = CalculateTotalPrice(b)
        }).ToList();
    }
    
    // for confirm attend the booking
    public async Task<(bool Success, string Message)> CheckInBookingAsync(int bookingId, int ownerId)
    {
        // Get booking with details 
        var booking = await _unitOfWork.BookingRepository.GetBookingWithDetailsAsync(bookingId);
        
        // Validate booking
        if (booking == null)
            return (false, "Booking not found");
            
        // Verify owner 
        var facilityOwner = await _unitOfWork.Repository<Facility>().GetByIdAsync(booking.Court.FacilityId);
        if (facilityOwner.OwnerId != ownerId)
            return (false, "You are not authorized to check in this booking");
            
        // Check booking is in confirmed status
        if (booking.status != BookingStatus.Confirmed)
            return (false, $"Cannot check in booking with status: {booking.status}");
            
        // Check if check-in is happening at an appropriate time
        var bookingTimeOnly = TimeOnly.FromTimeSpan(booking.StartTime);
        var bookingDateTime = booking.Date.ToDateTime(bookingTimeOnly);
        var currentTime = DateTime.Now;
        
        // Allow check-in within a reasonable window 
        var earlyWindow = bookingDateTime.AddMinutes(-15);
        var lateWindow = bookingDateTime.AddMinutes(30);
        
        if (currentTime < earlyWindow)
            return (false, "It's too early to check in for this booking");
            
        if (currentTime > lateWindow)
            return (false, "Check-in window has passed for this booking");
            
        // Update status to completed 
        booking.status = BookingStatus.Completed;
        booking.CheckInTime = DateTime.Now;
        
        _unitOfWork.Repository<Booking>().Update(booking);
        
     
        var result = await _unitOfWork.Complete();
        if (result <= 0) return (false, "Failed to check in booking");
        
        return (true, "User successfully checked in");
    }
    
    
    public async Task<IEnumerable<BookingDto>> GetBookingsForFacilityAsync(int facilityId, DateOnly date)
    {
        // Get all bookings for the facility with specific date 
        var bookings = await _unitOfWork.BookingRepository.GetBookingsForFacilityAsync(facilityId, date);
    
       
        return bookings.Select(b => new BookingDto
        {
            Id = b.Id,
            CourtId = b.CourtId,
            CourtName = b.Court.Name,
            FacilityName = b.Court.Facility.Name,
            Date = b.Date,
            StartTime = b.StartTime,
            EndTime = b.EndTime,
            Status = b.status.ToString(),
            TotalPrice = CalculateTotalPrice(b),
            UserId = b.UserId,
            UserName = b.User?.FirstName+"_"+b.User?.LastName 
        }).ToList();
    }
    private decimal CalculateTotalPrice(Booking booking)
    {
        if (booking.Court == null) 
            return 0;
        
        var durationHours = (booking.EndTime - booking.StartTime).TotalHours;
        return (decimal)durationHours * booking.Court.PricePerHour;
    }
    
    
}