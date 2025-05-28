using backend.Core.Entities;
using backend.Core.Interfaces;

namespace backend.Core.Specification
{
    public class UserRoleSpecification : BaseSpecification<UserRole>
    {
        public UserRoleSpecification(string roleName) : base(ur => ur.RoleName == roleName)
        {
        }
    }
} 