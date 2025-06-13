using System;

namespace backend.Api.DTOs.Booking;

public class SlotBlockDto
{
    public int Id { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    
}