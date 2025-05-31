using backend.Api.DTOs.Booking;

public class BookingResponseDto
{
    public int CourtId { get; set; }
    public DateOnly StartDate { get; set; }
    // public TimeSpan OpeningTime { get; set; }
    // public TimeSpan ClosingTime { get; set; }
    public Dictionary<string, List<SlotBlockDto>> BookingSlots { get; set; }
}