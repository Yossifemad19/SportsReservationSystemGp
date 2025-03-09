using System.Text.Json.Serialization;
using backend.Api.Helpers;

namespace backend.Api.DTOs;

public class FacilityDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    [JsonConverter(typeof(TimeSpanConverter))]
    public TimeSpan OpeningTime { get; set; }
    [JsonConverter(typeof(TimeSpanConverter))]
    public TimeSpan ClosingTime { get; set; }
    public int OwnerId { get; set; }  
    public AddressDto Address { get; set; }
}
