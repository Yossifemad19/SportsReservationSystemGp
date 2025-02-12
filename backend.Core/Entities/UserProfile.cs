namespace backend.Core.Entities;

public class UserProfile
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    public int UserCredentialId { get; set; }
    public UserCredential UserCredential { get; set; }
}