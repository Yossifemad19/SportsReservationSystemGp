using System.Text.Json.Serialization;
using backend.Api.Helpers;

namespace backend.Api.DTOs.Booking;

public class BookingRequestDto
{
    public int CourtId { get; set; }
    public DateOnly Date { get; set; }
    [JsonConverter(typeof(TimeSpanConverter))]
    public TimeSpan StartTime { get; set; }
    [JsonConverter(typeof(TimeSpanConverter))]
    public TimeSpan EndTime { get; set; }
    
}