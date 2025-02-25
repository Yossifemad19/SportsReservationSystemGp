using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Core.Entities;

public class Sport: BaseEntity
{
    [Required,Column(TypeName = "varchar(20)")]
    public string Name { get; set; }
    public ICollection<Court> Courts { get; set; }
}