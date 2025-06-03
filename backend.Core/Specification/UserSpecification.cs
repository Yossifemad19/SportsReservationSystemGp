using backend.Core.Entities;
using backend.Core.Interfaces;

namespace backend.Core.Specification
{
    public class UserSpecification : BaseSpecification<User>
    {
        public UserSpecification(string email) : base(o => o.Email == email)
        {
            AddInclude(o => o.UserRole);
        }
    }
}
