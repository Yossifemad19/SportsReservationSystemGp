using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backend.Core.Entities;
using backend.Core.Interfaces;

namespace backend.Repository.Data;

public static class SeedBookings
{
    public static async Task<int> SeedBookingsData(IUnitOfWork unitOfWork)
    {
        var bookingRepo = unitOfWork.Repository<Booking>();
        var courtRepo = unitOfWork.Repository<Court>();
        var userRepo = unitOfWork.Repository<User>();

        var existingBookings = await bookingRepo.GetAllAsync();
        if (existingBookings.Any())
        {
            return 0;
        }

        var courts = (await courtRepo.GetAllAsync()).ToList();
        var users = (await userRepo.GetAllAsync()).ToList();

        if (!courts.Any() || !users.Any())
        {
            return -1;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var bookings = new List<Booking>
       {
           new Booking
           {
               UserId = users[0].Id,
               CourtId = courts[0].Id,
               Date = new DateOnly(2025, 6, 15),
               StartTime = new TimeSpan(18, 0, 0),
               EndTime = new TimeSpan(20, 0, 0),
               status = BookingStatus.Confirmed,
               CheckInTime = null
           },
           new Booking
           {
               UserId = users[1].Id,
               CourtId = courts[1].Id,
               Date = new DateOnly(2025, 6, 15),  
               StartTime = new TimeSpan(14, 0, 0),
               EndTime = new TimeSpan(15, 0, 0),
               status = BookingStatus.Pending,
               CheckInTime = null
           }
       };

        foreach (var booking in bookings)
        {
            bookingRepo.Add(booking);
        }

        return await unitOfWork.Complete();
    }
}

