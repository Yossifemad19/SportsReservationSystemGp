using backend.Core.Entities;
using backend.Core.Interfaces;

namespace backend.Core.Specification
{
    public class AdminSpecification : BaseSpecification<Admin>
    {
        public AdminSpecification(string email) : base(a => a.Email == email)
        {
            AddInclude(a => a.UserRole);
        }
    }
} 