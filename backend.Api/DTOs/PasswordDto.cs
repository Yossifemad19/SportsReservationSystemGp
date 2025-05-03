using System.ComponentModel.DataAnnotations;

namespace backend.Api.DTOs;

public class PasswordDto
{
    public string Token { get; set; }
    [Required(ErrorMessage = "New password is required")]
    public string NewPassword { get; set; }
    [Required(ErrorMessage = "Confirm password is required")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
}

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }
}

