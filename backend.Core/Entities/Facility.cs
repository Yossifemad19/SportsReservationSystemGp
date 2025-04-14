using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Core.Entities;

public class Facility:BaseEntity
{
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    [Column(TypeName = "interval")]
    public TimeSpan OpeningTime { get; set; }
    [Column(TypeName = "interval")]
    public TimeSpan ClosingTime { get; set; }
    public int OwnerId { get; set; }
    public Owner Owner { get; set; }
    public Address Address { get; set; }
    public ICollection<Court> Courts { get; set; }
}