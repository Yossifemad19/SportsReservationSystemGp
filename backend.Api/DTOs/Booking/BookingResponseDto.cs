namespace backend.Api.DTOs.Booking;

public class BookingResponseDto
{
    public int CourtId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate => StartDate.AddDays(7);
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public object BookingSlots { get; set; }
}