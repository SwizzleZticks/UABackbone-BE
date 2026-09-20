using UABackbone_Backend.DTOs;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Services
{
    public class UserMapperService : IUserMapper
    {
        public UserDto ToUserDto(User user)
        {
            return new UserDto
            {
                Id                = user.Id,
                Username          = user.Username,
                FirstName         = user.FirstName,
                LastName          = user.LastName,
                Email             = user.Email,
                Local             = user.LocalId,
                IsAdmin           = user.IsAdmin,
                IsBlacklisted     = user.IsBlacklisted,
                IsBusinessAgent   = user.IsBusinessAgent,
                IsBusinessManager = user.IsBusinessManager

            };
        }

        public PendingUserDto ToPendingUserDto(PendingUser user)
        {
            return new PendingUserDto
            {
                Id          = user.Id,
                Username    = user.Username,
                FirstName   = user.FirstName,
                LastName    = user.LastName,
                Email       = user.Email,
                Local       = user.Local,
                SubmittedAt = user.SubmittedAt
            };
        }

        public BlacklistedUserDto ToBlacklistedUserDto(BlacklistedUser blacklist)
        {
            return new BlacklistedUserDto
            {
                Id = blacklist.Id,
                UserAffected = ToUserDto(blacklist.UserAffected),
                ByAdmin = ToUserDto(blacklist.ByAdmin),
                Reason = blacklist.Reason,
                Date = blacklist.Date
            };
        }
    }
}
