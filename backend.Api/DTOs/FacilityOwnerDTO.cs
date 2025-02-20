using System.ComponentModel.DataAnnotations;

namespace backend.Api.DTOs;

public class FacilityOwnerDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string PhoneNumber { get; set; }
    public string FacilitiesLocation { get; set; }
    public int FacilitiesNumber { get; set; }
}
