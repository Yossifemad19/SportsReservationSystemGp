using System.ComponentModel.DataAnnotations;

namespace backend.Core.Entities;

public class UserCredential
{
    public int  Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserProfile? UserProfile { get; set; }
}