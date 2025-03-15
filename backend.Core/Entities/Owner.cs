using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend.Core.Entities;

public class Owner : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PasswordHash { get; set; }
    public required ICollection<Facility> Facilities { get; set; }
    public UserRole UserRole { get; set; }
    public bool IsApproved { get; set; }
}