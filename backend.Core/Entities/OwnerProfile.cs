using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend.Core.Entities;

public class OwnerProfile
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string FacilitiesLocation { get; set; }
    public int FacilitiesNumber { get; set; }
    public bool IsActive { get; set; }
    public int UserCredentialId { get; set; }
    public UserCredential UserCredential { get; set; }
}


