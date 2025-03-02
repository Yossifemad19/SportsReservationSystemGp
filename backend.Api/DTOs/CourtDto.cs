using backend.Core.Entities;

namespace backend.Api.DTOs;

public class CourtDto
{
    public string Name { get; set; }
    public int FacilityId { get; set; }
    public int SportId { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerHour { get; set; }
}
