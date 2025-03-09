namespace backend.Api.DTOs.Booking;

public class SlotBlockDto
{
    // public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}