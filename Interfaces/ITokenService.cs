using UABackbone_Backend.Models;

namespace UABackbone_Backend.Interfaces;

public interface ITokenService
{
    string CreateToken(User aUser, int hoursUntilExpiration = 24, string purpose = "auth");
}