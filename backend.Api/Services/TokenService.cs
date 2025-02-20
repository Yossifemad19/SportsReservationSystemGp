using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Core.Entities;
using Microsoft.IdentityModel.Tokens;


namespace backend.Api.Services;

public class TokenService:ITokenService
{
    private readonly SymmetricSecurityKey _signingKey;
    private readonly string _issuer;
    private readonly string _audience;
    public TokenService(IConfiguration config)
    {
        var jwtSettings = config.GetSection("jwt");
        var secretKey = jwtSettings["SecretKey"];
        _issuer = jwtSettings["Issuer"];
        _audience = jwtSettings["Audience"];

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT secret key is not set in configuration.");
        }

        _signingKey =new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }
    public string GenerateToken(UserProfile user) 
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateToken(OwnerProfile owner)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, owner.Id.ToString()),

        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}