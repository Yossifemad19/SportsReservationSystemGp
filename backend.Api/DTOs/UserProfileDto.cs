using System.ComponentModel.DataAnnotations;

namespace backend.Api.DTOs;

public class UserProfileDto
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required]
    public string? UserName { get; set; }
    [EmailAddress]
    public string Email { get; set; }

    [Phone]
    public string PhoneNumber { get; set; }
    
}
