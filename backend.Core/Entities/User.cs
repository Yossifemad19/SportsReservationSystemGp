using System.ComponentModel.DataAnnotations;

namespace backend.Core.Entities;

public class User : BaseEntity
{
    [Required]
    public string UserName { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    [Required]
    public string PasswordHash { get; set; }
    
    [Required]
    public int UserRoleId { get; set; }
    public UserRole UserRole { get; set; }
    
    public PlayerProfile PlayerProfile { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; }


    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }

    public bool IsBlocked { get; set; } = false;
    public DateTime? BlockEndDate { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}