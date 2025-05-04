using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Core.Entities;

public class Admin : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    [Required]
    public string Email { get; set; }
    
    [Required]
    public string PasswordHash { get; set; }
    
    [Required]
    public int UserRoleId { get; set; }
    public UserRole UserRole { get; set; }
}

