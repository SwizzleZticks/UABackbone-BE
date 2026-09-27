using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers
{
    [Route("api/pending-users")]
    [ApiController]
    public class PendingUsersController(RailwayContext context, IUserMapper userMapperService) : BaseApiController
    {
        [HttpGet("uacard/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUaCardAsync(int id)
        {
            var user = await context.PendingUsers.FindAsync(id);
            if (user is null)
            {
                return NotFound("User not found");
            }

            return File(user.UaCardImage, "image/jpeg");
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PendingUserDto>> GetPendingUserByIdAsync(int id)
        {
            var user = await context.PendingUsers.FindAsync(id);

            return user != null 
                ? Ok(userMapperService.ToPendingUserDto(user)) 
                : NotFound("User not found");
        }

        [HttpGet("all-pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PendingUserDto>> GetAllPendingUsersAsync()
        {
            var pendingUsers = await context.PendingUsers.ToListAsync();
            List<PendingUserDto> pendingUsersDtos = new List<PendingUserDto>();
            foreach (var pendingUser in pendingUsers)
            {
                var pendingUserDto = userMapperService.ToPendingUserDto(pendingUser);
                pendingUsersDtos.Add(pendingUserDto);
            }

            return Ok(pendingUsersDtos);
        }

        [HttpGet("all-pending-paginated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<PendingUserDto>>> GetPendingUsersPaginated(
            int page = 1,
            int limitSize = 25,
            string? searchTerm = null)
        {
            var pendingUsers = context.PendingUsers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalized = searchTerm.Trim().ToLower();

                pendingUsers = pendingUsers.Where(u =>
                    u.Id.ToString().Contains(normalized) ||
                    u.FirstName.ToLower().Contains(normalized) ||
                    u.LastName.ToLower().Contains(normalized) ||
                    u.Email.ToLower().Contains(normalized) ||
                    u.Username.ToLower().Contains(normalized) ||
                    u.Local.ToString().Contains(normalized));
            }

            var totalCount = await pendingUsers.CountAsync();

            var allPendingUsers = await pendingUsers
                .Skip((page - 1) * limitSize)
                .Take(limitSize)
                .ToListAsync();

            List<PendingUserDto> pendingUsersDtos = new List<PendingUserDto>();

            foreach (var pendingUser in allPendingUsers)
            {
                var pendingUserDto = userMapperService.ToPendingUserDto(pendingUser);
                pendingUsersDtos.Add(pendingUserDto);
            }

            return Ok(new PagedResultDto<PendingUserDto>
            {
                Total = totalCount,
                Items = pendingUsersDtos
            });
        }
    }
}
