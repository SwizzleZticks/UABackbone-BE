using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(User aUser, int hoursUntilExpiration = 24, string purpose = "auth")
    {
        var tokenKey = Environment.GetEnvironmentVariable("TOKEN_KEY") ?? throw new Exception("Cannot access Token Key");

        if (tokenKey.Length < 64)
        {
            throw new Exception("Token Key does not meet minimum length of 64 characters.");
        }
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
        var claims = new List<Claim>();

        if (aUser.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, "User"));
        }

        claims.Add(new Claim(ClaimTypes.Sid, aUser.Id.ToString()));
        claims.Add(new Claim(ClaimTypes.NameIdentifier, aUser.Username));
        claims.Add(new Claim("token_type", purpose));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(hoursUntilExpiration),
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(descriptor);
        
        return tokenHandler.WriteToken(token);
    }
}