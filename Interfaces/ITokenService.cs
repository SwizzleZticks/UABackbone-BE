using UABackbone_Backend.Models;

namespace UABackbone_Backend.Interfaces;

public interface ITokenService
{
    string CreateToken(User aUser);
}