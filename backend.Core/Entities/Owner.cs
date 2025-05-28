using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Core.Entities;

public class Owner : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    public string PhoneNumber { get; set; }
    
    [Required]
    public string PasswordHash { get; set; }
    
    [Required]
    public int UserRoleId { get; set; }
    public UserRole UserRole { get; set; }
    
    public bool IsApproved { get; set; }
    
    public ICollection<Facility> Facilities { get; set; } = new List<Facility>();
}