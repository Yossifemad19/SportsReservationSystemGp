namespace backend.Api.DTOs.Booking;

public class BookingDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }

    public int CourtId { get; set; }
    public string CourtName { get; set; }
    public string FacilityName { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string City { get; set; }
    public string Status { get; set; }
    public decimal TotalPrice { get; set; }
}