namespace backend.Core.Entities;

public class Booking:BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }
    public int CourtId { get; set; }
    public Court Court { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingStatus status { get; set; }
    public decimal TotalPrice => (EndTime.Hour - StartTime.Hour)*Court.PricePerHour;
}