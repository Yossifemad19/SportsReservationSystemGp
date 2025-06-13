using System;

namespace backend.Api.DTOs.Booking;

public class SlotBlockDto
{
    public int Id { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    
}