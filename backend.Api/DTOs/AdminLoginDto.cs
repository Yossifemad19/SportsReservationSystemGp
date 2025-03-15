using System.ComponentModel.DataAnnotations;

namespace backend.Api.DTOs;

public class AdminLoginDto
{
    [Required(ErrorMessage = "Email Is Required"), EmailAddress(ErrorMessage = "Invalid Email Format")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password Is Required")]
    public string Password { get; set; }
}
