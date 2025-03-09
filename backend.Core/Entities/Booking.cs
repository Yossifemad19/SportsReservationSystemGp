using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Core.Entities;

public class Booking:BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }
    public int CourtId { get; set; }
    public Court Court { get; set; }
    
    public DateOnly Date { get; set; }
    [Column(TypeName = "interval")]
    public TimeSpan StartTime { get; set; }
    [Column(TypeName = "interval")]
    public TimeSpan EndTime { get; set; }
    public BookingStatus status { get; set; }
    // public decimal TotalPrice => (EndTime - StartTime)*Court.PricePerHour;
}