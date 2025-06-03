using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Core.Entities;
using backend.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Api.Services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _signingKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly IUnitOfWork _unitOfWork;

    public TokenService(IConfiguration config, IUnitOfWork unitOfWork)
    {
        var jwtSettings = config.GetSection("jwt");
        var secretKey = jwtSettings["SecretKey"];
        _issuer = jwtSettings["Issuer"];
        _audience = jwtSettings["Audience"];
        _unitOfWork = unitOfWork;

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT secret key is not set in configuration.");
        }

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    public string GenerateToken(User user)
    {
        // Get role name asynchronously but execute synchronously
        var userRole = _unitOfWork.Repository<UserRole>().GetByIdAsync(user.UserRoleId).GetAwaiter().GetResult();
        var roleName = userRole?.RoleName ?? "Unknown";

        var claims = new[]
        {
            new Claim("sub", user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, roleName),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(1440),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateToken(Owner owner)
    {
        // Get role name asynchronously but execute synchronously
        var userRole = _unitOfWork.Repository<UserRole>().GetByIdAsync(owner.UserRoleId).GetAwaiter().GetResult();
        var roleName = userRole?.RoleName ?? "Unknown";

        var claims = new[]
        {
            new Claim("sub", owner.Id.ToString()),
            new Claim(ClaimTypes.Email, owner.Email),
            new Claim(ClaimTypes.Role, roleName),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(1440),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateToken(Admin admin)
    {
        // Get role name asynchronously but execute synchronously
        var userRole = _unitOfWork.Repository<UserRole>().GetByIdAsync(admin.UserRoleId).GetAwaiter().GetResult();
        var roleName = userRole?.RoleName ?? "Unknown";

        var claims = new[]
        {
            new Claim("sub", admin.Id.ToString()),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim(ClaimTypes.Role, roleName),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(1440),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}