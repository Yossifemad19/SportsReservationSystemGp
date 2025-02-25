namespace backend.Core.Entities;

public class Address:BaseEntity
{
    public string StreetAddress { get; set; }
    public string City { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}