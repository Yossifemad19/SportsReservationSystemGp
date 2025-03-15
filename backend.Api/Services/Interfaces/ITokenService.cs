using backend.Core.Entities;

namespace backend.Api.Services;

public interface ITokenService
{
    string GenerateToken(User user);
    string GenerateToken(Owner owner);
    string GenerateToken(Admin admin);
    // string GenerateToken(OwnerProfile owner);

}