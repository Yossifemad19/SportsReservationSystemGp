using backend.Core.Interfaces;
using backend.Core.Entities;
using backend.Api.DTOs.Booking;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Api.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private const int NO_SHOW_BLOCK_DAYS = 30; 

    public BookingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

   public async Task<(bool Success, string Message)> BookCourtAsync(object request, int userId)
{
    var bookingRequest = (BookingRequestDto)request;

    var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
    if (user == null)
        return (false, "User not found");

    if (user.IsBlocked && user.BlockEndDate > DateTime.Now)
        return (false, $"You are blocked until {user.BlockEndDate:yyyy-MM-dd} due to previous no-shows");

    if (user.IsBlocked && user.BlockEndDate <= DateTime.Now)
    {
        user.IsBlocked = false;
        user.BlockEndDate = null;
        _unitOfWork.Repository<User>().Update(user);
    }

    var court = await _unitOfWork.Repository<Court>().GetByIdAsync(bookingRequest.CourtId);
    if (court == null)
        return (false, "Court not found");

    var booking = new Booking
    {
        UserId = userId,
        CourtId = bookingRequest.CourtId,
        Date = bookingRequest.Date,
        StartTime = bookingRequest.StartTime,
        EndTime = bookingRequest.EndTime,
        status = BookingStatus.Pending,
        CheckInTime = DateTime.UtcNow
    };

    using var transaction = await _unitOfWork.BeginTransactionAsync();
    try
    {
        // ✅ Check for overlapping bookings inside the transaction (with row-level lock)
        var hasConflict = await _unitOfWork.BookingRepository.HasConflictAsync(
            booking.CourtId, booking.Date, booking.StartTime, booking.EndTime);

        if (hasConflict)
        {
            await transaction.RollbackAsync();
            return (false, "Court is not available for the requested time");
        }

        _unitOfWork.Repository<Booking>().Add(booking);
        var result = await _unitOfWork.Complete();

        if (result <= 0)
        {
            await transaction.RollbackAsync();
            return (false, "Failed to create booking");
        }

        await transaction.CommitAsync();
        return (true, "Booking created successfully");
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return (false, $"Error creating booking: {ex.Message}");
    }
}

    public async Task<object> GetBookingsForCourtAsync(int courtId, DateOnly date)
    {
        var bookings = await _unitOfWork.BookingRepository.GetBookingsForCourtAsync(courtId, date);
        
      var bookingSlots = bookings
    .GroupBy(b => b.Date)
    .OrderBy(group => group.Key)
    .ToDictionary(
        group => group.Key.ToString("yyyy-MM-dd"),
        group => group.OrderBy(b => b.StartTime)
            .Select(b => new SlotBlockDto
            {
                Id = b.Id,
                UserFullName = $"{b.User.FirstName} {b.User.LastName}",
                StartTime = b.StartTime.ToString(@"hh\:mm\:ss"),
                EndTime = b.EndTime.ToString(@"hh\:mm\:ss")
            })
            .ToList()
    );

return new BookingResponseDto
{
    CourtId = courtId,
    StartDate = date,
    BookingSlots = bookingSlots
};

    }

    public async Task<(bool Success, string Message)> CancelBookingAsync(int bookingId, int userId)
    {

        var booking = await _unitOfWork.BookingRepository.GetBookingWithDetailsAsync(bookingId);
        
        if (booking == null)
            return (false, "Booking not found");
        
        if (booking.UserId != userId)
            return (false, "You are not authorized to cancel this booking");
        
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

        var booking = await _unitOfWork.BookingRepository.GetBookingWithDetailsAsync(bookingId);

        if (booking == null)
            return (false, "Booking not found");
        
        if (booking.UserId != userId)
            return (false, "You are not authorized to confirm this booking");
        
        if (booking.status != BookingStatus.Pending)
            return (false, $"Cannot confirm booking with status: {booking.status}");
        
        booking.status = BookingStatus.Confirmed;
        
        _unitOfWork.Repository<Booking>().Update(booking);
        
        // Save changes
        var result = await _unitOfWork.Complete();
        if (result <= 0) return (false, "Failed to confirm booking");
        
        return (true, "Booking confirmed successfully");
    }
    
    public async Task<IEnumerable<object>> GetUserBookingsAsync(int userId, BookingStatus? status = null)
    {
        // Get user bookings 
        var bookings = await _unitOfWork.BookingRepository.GetUserBookingsAsync(userId, status);
        
        return bookings.Select(b => new BookingDto
        {
            Id = b.Id,
            UserName = $"{b.User.FirstName} {b.User.LastName}",
            UserId = b.UserId,
            CourtName = b.Court.Name,
            FacilityName = b.Court.Facility.Name,
            Date = b.Date,
            StartTime = b.StartTime,
            City = b.Court.Facility.Address.City,
            Status = b.status.ToString(),
            EndTime = b.EndTime,
            TotalPrice = CalculateTotalPrice(b),
            CheckIn = b.CheckInTime
        });
    }
    
    public async Task<(bool Success, string Message)> CheckInBookingAsync(int bookingId, int ownerId)
    {
        var booking = await _unitOfWork.BookingRepository.GetBookingWithDetailsAsync(bookingId);
        
        if (booking == null)
            return (false, "Booking not found");

        var facilityOwner = await _unitOfWork.Repository<Facility>().GetByIdAsync(booking.Court.FacilityId);
        if (facilityOwner.OwnerId != ownerId)
            return (false, "You are not authorized to check in this booking");

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
        booking.CheckInTime = DateTime.UtcNow;
        
        _unitOfWork.Repository<Booking>().Update(booking);
        
     
        var result = await _unitOfWork.Complete();
        if (result <= 0) return (false, "Failed to check in booking");
        
        return (true, "User successfully checked in");
    }
    
    
    public async Task<IEnumerable<object>> GetBookingsForFacilityAsync(int facilityId, DateOnly date)
    {
        // Get bookings for facility with specific date
        var bookings = await _unitOfWork.BookingRepository.GetBookingsForFacilityAsync(facilityId, date);
        
        return bookings.Select(b => new BookingDto
        {
            Id = b.Id,
            UserName = $"{b.User.FirstName} {b.User.LastName}",
            UserId = b.UserId,
            CourtName = b.Court.Name,
            FacilityName = b.Court.Facility.Name,
            Date = b.Date,
            StartTime = b.StartTime,
            City = b.Court.Facility.Address.City,
            Status = b.status.ToString(),
            EndTime = b.EndTime,
            TotalPrice = CalculateTotalPrice(b),
            CheckIn = b.CheckInTime
        });
    }
    private decimal CalculateTotalPrice(Booking booking)
    {
        if (booking.Court == null) 
            return 0;
        
        var durationHours = (booking.EndTime - booking.StartTime).TotalHours;
        return (decimal)durationHours * booking.Court.PricePerHour;
    }
    
    public async Task HandleNoShowsAsync()
    {
        var currentTime = DateTime.Now;
        var currentDate = DateOnly.FromDateTime(currentTime);
        var currentTimeOnly = TimeOnly.FromDateTime(currentTime);
        
        var noShowBookings = await _unitOfWork.BookingRepository.GetNoShowBookingsAsync(currentDate, currentTimeOnly);

        foreach (var booking in noShowBookings)
        {
            booking.status = BookingStatus.NoShow;
            _unitOfWork.Repository<Booking>().Update(booking);
            
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(booking.UserId);
            if (user != null)
            {
                user.IsBlocked = true;
                user.BlockEndDate = DateTime.Now.AddDays(NO_SHOW_BLOCK_DAYS);
                _unitOfWork.Repository<User>().Update(user);
            }
        }

        await _unitOfWork.Complete();
    }
}