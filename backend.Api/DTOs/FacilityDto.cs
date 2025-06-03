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

    public IFormFile Image { get; set; }
    public string? ImageUrl { get; set; }

    public int OwnerId { get; set; }
    public AddressDto Address { get; set; }
}
