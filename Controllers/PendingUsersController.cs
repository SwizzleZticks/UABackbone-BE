using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers
{
    [Route("api/pending-users")]
    [ApiController]
    public class PendingUsersController(RailwayContext context) : BaseApiController
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
        public async Task<ActionResult<User>> GetPendingUserByIdAsync(int id)
        {
            var user = await context.PendingUsers.FindAsync(id);

            return user != null ? Ok(new PendingUserDto
            {
                Id          = user.Id,
                Username    = user.Username,
                Email       = user.Email,
                FirstName   = user.FirstName,
                LastName    = user.LastName,
                Local       = user.Local,
                SubmittedAt = user.SubmittedAt,
            }) : NotFound("User not found");
        }

        [HttpGet("all-pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PendingUserDto>> GetAllPendingUsersAsync()
        {
            var pendingUsers = await context.PendingUsers.ToListAsync();
            List<PendingUserDto> pendingUsersDtos = new List<PendingUserDto>();
            foreach (var pendingUser in pendingUsers)
            {
                var pendingUserDto = new PendingUserDto
                {
                    Id          = pendingUser.Id,
                    Username    = pendingUser.Username,
                    FirstName   = pendingUser.FirstName,
                    LastName    = pendingUser.LastName,
                    Email       = pendingUser.Email,
                    Local       = pendingUser.Local,
                    SubmittedAt = pendingUser.SubmittedAt
                };
                pendingUsersDtos.Add(pendingUserDto);
            }

            return Ok(pendingUsersDtos);
        }

        [HttpGet("all-pending-paginated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<PendingUserDto>>> GetPendingUsersPaginated(int page = 1, int limitSize = 25)
        {
            var pendingUsers = await context.PendingUsers
                .Skip((page - 1) * limitSize)
                .Take(limitSize)
                .AsQueryable()
                .ToListAsync();
            var totalCount = await context.PendingUsers.CountAsync();

            List<PendingUserDto> pendingUsersDtos = new List<PendingUserDto>();
            foreach (var pendingUser in pendingUsers)
            {
                var pendingUserDto = new PendingUserDto
                {
                    Id          = pendingUser.Id,
                    Username    = pendingUser.Username,
                    FirstName   = pendingUser.FirstName,
                    LastName    = pendingUser.LastName,
                    Email       = pendingUser.Email,
                    Local       = pendingUser.Local,
                    SubmittedAt = pendingUser.SubmittedAt
                };
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
