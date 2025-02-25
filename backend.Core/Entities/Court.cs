namespace backend.Core.Entities;

public class Court:BaseEntity
{
    public string Name { get; set; }
    public int FacilityId { get; set; }
    public Facility Facility { get; set; }
    public int SportId { get; set; }
    public Sport Sport { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerHour { get; set; }

    public ICollection<Booking> Bookings { get; set; }
}