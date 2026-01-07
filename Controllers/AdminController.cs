using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers;
[Authorize(Roles = "Admin")]
public class AdminController(RailwayContext context, IEmailService emailService, ITokenService tokenService) : BaseApiController
{
    [HttpPost("approve/{id}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> VerifyUserAsync(int id)
    {
        var pendingUser = await context.PendingUsers.FindAsync(id);
        if (pendingUser == null) return NotFound();

        var user = new User
        {
            Username      = pendingUser.Username.Trim().ToLowerInvariant(),
            PasswordHash  = pendingUser.PasswordHash,
            FirstName     = pendingUser.FirstName!.Trim(),
            LastName      = pendingUser.LastName!.Trim(),
            Email         = pendingUser.Email.Trim().ToLowerInvariant(),
            IsVerified    = true,
            IsAdmin       = false,
            IsBlacklisted = false,
            LocalId       = pendingUser.Local
        };

        context.PendingUsers.Remove(pendingUser);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        await emailService.SendApprovedAsync(user.Email, user.FirstName ?? "");

        var userDto = new AuthResponseDto
        {
            Id        = user.Id,
            Username  = user.Username,
            FirstName = user.FirstName!,
            LastName  = user.LastName!,
            Email     = user.Email,
            Local     = user.LocalId,
            Token     = tokenService.CreateToken(user)
        };

        return Created("api/Account/verify", userDto);
    }
    
    [HttpPost("reject/{id}/deny")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> DeletePendingUserAsync(int id, [FromBody]string reason)
    {
        var user = await context.PendingUsers.FindAsync(id);

        if (user == null)
        {
            return NotFound("User not found");
        }
        context.PendingUsers.Remove(user);
        await context.SaveChangesAsync();

        await emailService.SendDeniedAsync(user.Email, user.FirstName, reason);
        
        return NoContent();
    }

    [HttpPost("blacklist/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BlacklistUserAsync(int id, [FromBody]string reason)
    {
        var sidClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid);
        if (sidClaim is null || !int.TryParse(sidClaim.Value, out var adminId))
        {
            return Unauthorized("Missing or invalid admin identity.");
        }

        var admin = await context.Users.FindAsync(adminId);
        if (admin is null)
        {
            return NotFound("Admin not found.");
        }

        var user = await context.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound("User not found.");
        }

        if (user.IsBlacklisted || await context.BlacklistedUsers.AnyAsync(b => b.UserAffected.Id == id))
        {
            return Conflict("User is already blacklisted.");
        }

        user.IsBlacklisted = true;

        context.BlacklistedUsers.Add(new BlacklistedUser
        {
            UserAffected = user,
            ByAdmin      = admin,
            Reason       = reason.Trim(),
            Date         = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        return Ok(new UserDto
        {
            Id        = user.Id,
            Username  = user.Username,
            Email     = user.Email,
            FirstName = user.FirstName,
            LastName  = user.LastName,
            Local     = user.LocalId
        });
    }

    [HttpGet("blacklist/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BlacklistedUserDto>> GetBlackListUserAsync(int id)
    {
        var sidClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid);
        if (sidClaim is null || !int.TryParse(sidClaim.Value, out var adminId))
        {
            return Unauthorized("Missing or invalid admin identity.");
        }

        var admin = await context.Users.FindAsync(adminId);
        if (admin is null)
        {
            return NotFound("Admin not found.");
        }

        var blacklist = await context.BlacklistedUsers
            .Include(b => b.UserAffected)
            .Include(b => b.ByAdmin)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (blacklist == null)
        {
            return NotFound("Blacklist entry not found");
        }

        return Ok(new BlacklistedUserDto
        {
            Id = id,
            UserAffected = new UserDto
            {
                Id        = blacklist.UserAffected.Id,
                Username  = blacklist.UserAffected.Username,
                FirstName = blacklist.UserAffected.FirstName,
                LastName  = blacklist.UserAffected.LastName,
                Email     = blacklist.UserAffected.Email,
                Local     = blacklist.UserAffected.LocalId
            },
            ByAdmin = new UserDto
            {
                Id        = admin.Id,
                Username  = admin.Username,
                FirstName = admin.FirstName,
                LastName  = admin.LastName,
                Email     = admin.Email,
                Local     = admin.LocalId
            },
            Reason = blacklist.Reason,
            Date   = blacklist.Date,
        });
    }

    [HttpDelete("blacklist/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBlacklistedUser(int id)
    {
        var blacklistEntry = await context.BlacklistedUsers
            .Include(b => b.UserAffected)
            .Include(b => b.ByAdmin)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (blacklistEntry == null)
        {
            return NotFound("Blacklist entry not found");
        }

        var user = await context.Users.FindAsync(blacklistEntry.UserAffected.Id);
        if (user == null)
        {
            return NotFound("User not found");
        }

        user.IsBlacklisted = false;
        context.BlacklistedUsers.Remove(blacklistEntry);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("uacard/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUaCardAsync(int id)
    {
        var user =  await context.PendingUsers.FindAsync(id);
        if (user is null)
        {
            return NotFound("User not found");
        }
        
        return File(user.UaCardImage, "image/jpeg");
    }
    
    [HttpGet("paginated-users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResultDto<UserDto>>> GetUsersPaginatedAsync(int page = 1, int limitSize = 25)
    {
        var users = await context.Users
            .Skip((page - 1) * limitSize)
            .Take(limitSize)
            .AsQueryable()
            .ToListAsync();
        var totalCount = await context.Users.CountAsync();
        var userDtos = ConvertToUserDtos(users);

        return Ok(new PagedResultDto<UserDto>
        {
            Total = totalCount,
            Items = userDtos
        });
    }

    [HttpGet("all-pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    [HttpGet("all-users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> GetAllUsersAsync()
    {
        var users = await context.Users.ToListAsync();
        var userDtos = ConvertToUserDtos(users);

        return Ok(userDtos);
    }

    [HttpGet("user/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUserByIdAsync(int id)
    {
        var user = await context.Users.FindAsync(id);

        return user != null ? Ok(new UserDto
        {
            Id        = user.Id,
            Username  = user.Username,
            Email     = user.Email,
            FirstName = user.FirstName,
            LastName  = user.LastName,
            Local     = user.LocalId
        }) : NotFound("User not found");
    }

    [HttpDelete("delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUserAsync(int id)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound("User not found");
        }
        context.Users.Remove(user);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("admin-toggle/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PromoteUserAsync(int id)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound("User not found");
        }

        user.IsAdmin = !user.IsAdmin;
        await context.SaveChangesAsync();
        var newToken = tokenService.CreateToken(user);

        return Ok(new { token = newToken });
    }

    private List<UserDto> ConvertToUserDtos(List<User> users)
    {
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var userDto = new UserDto
            {
                Id        = user.Id,
                Username  = user.Username,
                Email     = user.Email,
                FirstName = user.FirstName,
                LastName  = user.LastName,
                Local     = user.LocalId
            };
            userDtos.Add(userDto);
        }

        return userDtos;
    }
}