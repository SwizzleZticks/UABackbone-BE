using UABackbone_Backend.DTOs;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Interfaces
{
    public interface IUserMapper
    {
        UserDto ToUserDto(User user);
        PendingUserDto ToPendingUserDto(PendingUser user);
        BlacklistedUserDto ToBlacklistedUserDto(BlacklistedUser blacklist);
    }
}
