namespace backend.Api.DTOs;

public class FacilityDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int OwnerId { get; set; }  
    public AddressDto Address { get; set; }
}
