using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Core.Entities;

public class Facility:BaseEntity
{
    public string Name { get; set; }
    public int OwnerId { get; set; }
    public Owner Owner { get; set; }
    public Address Address { get; set; }
    public ICollection<Court> Courts { get; set; }
}