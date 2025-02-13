using backend.Core.Entities;

namespace backend.Api.Services;

public interface ITokenService
{
    string GenerateToken(UserProfile user);
    
}