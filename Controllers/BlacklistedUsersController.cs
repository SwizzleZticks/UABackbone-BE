using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers
{
    [Route("api/blacklisted-users")]
    [ApiController]
    public class BlacklistedUsersController(RailwayContext context) : BaseApiController
    {

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BlacklistedUserDto>> GetBlacklistedUserAsync(int id)
        {
            var blacklist = await context.BlacklistedUsers
                .Include(b => b.UserAffected)
                .Include(b => b.ByAdmin)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blacklist is null)
            {
                return NotFound("Blacklist entry not found.");
            }

            var blacklistDto = new BlacklistedUserDto
            {
                Id = blacklist.Id,

                UserAffected = new UserDto
                {
                    Id            = blacklist.UserAffected.Id,
                    Username      = blacklist.UserAffected.Username,
                    FirstName     = blacklist.UserAffected.FirstName,
                    LastName      = blacklist.UserAffected.LastName,
                    Email         = blacklist.UserAffected.Email,
                    Local         = blacklist.UserAffected.LocalId,
                    IsAdmin       = blacklist.UserAffected.IsAdmin,
                    IsBlacklisted = blacklist.UserAffected.IsBlacklisted
                },

                ByAdmin = new UserDto
                {
                    Id            = blacklist.ByAdmin.Id,
                    Username      = blacklist.ByAdmin.Username,
                    FirstName     = blacklist.ByAdmin.FirstName,
                    LastName      = blacklist.ByAdmin.LastName,
                    Email         = blacklist.ByAdmin.Email,
                    Local         = blacklist.ByAdmin.LocalId,
                    IsAdmin       = blacklist.ByAdmin.IsAdmin,
                    IsBlacklisted = blacklist.ByAdmin.IsBlacklisted
                },

                Reason = blacklist.Reason,
                Date   = blacklist.Date
            };

            return Ok(blacklistDto);
        }


        [HttpGet("all-blacklisted")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BlacklistedUserDto>>> GetAllBlacklistedUsersAsync()
        {
            var blacklistedUsers = await context.BlacklistedUsers
                .Include(b => b.UserAffected)
                .Include(b => b.ByAdmin)
                .ToListAsync();

            var blacklistedUserDtos = new List<BlacklistedUserDto>();

            foreach (var blacklistedUser in blacklistedUsers)
            {
                var blacklistedUserDto = new BlacklistedUserDto
                {
                    Id = blacklistedUser.Id,

                    UserAffected = new UserDto
                    {
                        Id            = blacklistedUser.UserAffected.Id,
                        Username      = blacklistedUser.UserAffected.Username,
                        FirstName     = blacklistedUser.UserAffected.FirstName,
                        LastName      = blacklistedUser.UserAffected.LastName,
                        Email         = blacklistedUser.UserAffected.Email,
                        Local         = blacklistedUser.UserAffected.LocalId,
                        IsAdmin       = blacklistedUser.UserAffected.IsAdmin,
                        IsBlacklisted = blacklistedUser.UserAffected.IsBlacklisted
                    },

                    ByAdmin = new UserDto
                    {
                        Id            = blacklistedUser.ByAdmin.Id,
                        Username      = blacklistedUser.ByAdmin.Username,
                        FirstName     = blacklistedUser.ByAdmin.FirstName,
                        LastName      = blacklistedUser.ByAdmin.LastName,
                        Email         = blacklistedUser.ByAdmin.Email,
                        Local         = blacklistedUser.ByAdmin.LocalId,
                        IsAdmin       = blacklistedUser.ByAdmin.IsAdmin,
                        IsBlacklisted = blacklistedUser.ByAdmin.IsBlacklisted
                    },

                    Reason = blacklistedUser.Reason,
                    Date   = blacklistedUser.Date
                };

                blacklistedUserDtos.Add(blacklistedUserDto);
            }

            return Ok(blacklistedUserDtos);
        }

        [HttpGet("all-blacklisted-paginated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<BlacklistedUserDto>>> GetBlacklistedUsersPaginated(int page = 1, int limitSize = 25)
        {
            var blacklistedUsers = await context.BlacklistedUsers
                .Include(b => b.UserAffected)
                .Include(b => b.ByAdmin)
                .Skip((page - 1) * limitSize)
                .Take(limitSize)
                .ToListAsync();

            var totalCount = await context.BlacklistedUsers.CountAsync();

            var blacklistedUserDtos = new List<BlacklistedUserDto>();

            foreach (var blacklistedUser in blacklistedUsers)
            {
                var blacklistedUserDto = new BlacklistedUserDto
                {
                    Id = blacklistedUser.Id,

                    UserAffected = new UserDto
                    {
                        Id            = blacklistedUser.UserAffected.Id,
                        Username      = blacklistedUser.UserAffected.Username,
                        FirstName     = blacklistedUser.UserAffected.FirstName,
                        LastName      = blacklistedUser.UserAffected.LastName,
                        Email         = blacklistedUser.UserAffected.Email,
                        Local         = blacklistedUser.UserAffected.LocalId,
                        IsAdmin       = blacklistedUser.UserAffected.IsAdmin,
                        IsBlacklisted = blacklistedUser.UserAffected.IsBlacklisted
                    },

                    ByAdmin = new UserDto
                    {
                        Id            = blacklistedUser.ByAdmin.Id,
                        Username      = blacklistedUser.ByAdmin.Username,
                        FirstName     = blacklistedUser.ByAdmin.FirstName,
                        LastName      = blacklistedUser.ByAdmin.LastName,
                        Email         = blacklistedUser.ByAdmin.Email,
                        Local         = blacklistedUser.ByAdmin.LocalId,
                        IsAdmin       = blacklistedUser.ByAdmin.IsAdmin,
                        IsBlacklisted = blacklistedUser.ByAdmin.IsBlacklisted
                    },

                    Reason = blacklistedUser.Reason,
                    Date   = blacklistedUser.Date
                };

                blacklistedUserDtos.Add(blacklistedUserDto);
            }

            return Ok(new PagedResultDto<BlacklistedUserDto>
            {
                Total = totalCount,
                Items = blacklistedUserDtos
            });
        }
    }
}
