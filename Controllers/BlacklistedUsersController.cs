using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers
{
    [Route("api/blacklisted-users")]
    [ApiController]
    public class BlacklistedUsersController(RailwayContext context, IUserMapper userMapperService) : BaseApiController
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

            return Ok(userMapperService.ToBlacklistedUserDto(blacklist));
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
                var blacklistedUserDto = userMapperService.ToBlacklistedUserDto(blacklistedUser);

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
                var blacklistedUserDto = userMapperService.ToBlacklistedUserDto(blacklistedUser);

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
