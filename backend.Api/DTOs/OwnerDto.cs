using System.ComponentModel.DataAnnotations;

namespace backend.Api.DTOs;

public class OwnerRegisterDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string? PhoneNumber { get; set; }

}

public class OwnerLoginDto 
{
    [Required(ErrorMessage = "Email Is Required"), EmailAddress(ErrorMessage = "Invalid Email Format")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password Is Required")]
    public string Password { get; set; }
}

public class UnApprovedOwnerDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public bool IsApproved { get; set; }
}
