using System.ComponentModel.DataAnnotations;

namespace backend.Core.Entities
{
    public class UserRole : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; }
        
        public string Description { get; set; }
        
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}