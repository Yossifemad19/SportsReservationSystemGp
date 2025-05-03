namespace backend.Core.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public UserRole UserRole { get; set; }
    public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Booking> Bookings { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }


}