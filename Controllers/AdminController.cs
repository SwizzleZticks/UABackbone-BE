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
            Id            = user.Id,
            Username      = user.Username,
            FirstName     = user.FirstName!,
            LastName      = user.LastName!,
            Email         = user.Email,
            Local         = user.LocalId,
            IsAdmin       = user.IsAdmin,
            IsBlacklisted = user.IsBlacklisted,
            Token         = tokenService.CreateToken(user)
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
            Id            = user.Id,
            Username      = user.Username,
            Email         = user.Email,
            FirstName     = user.FirstName,
            LastName      = user.LastName,
            Local         = user.LocalId,
            IsAdmin       = user.IsAdmin,
            IsBlacklisted = user.IsBlacklisted,
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
                Id            = blacklist.UserAffected.Id,
                Username      = blacklist.UserAffected.Username,
                FirstName     = blacklist.UserAffected.FirstName,
                LastName      = blacklist.UserAffected.LastName,
                Email         = blacklist.UserAffected.Email,
                Local         = blacklist.UserAffected.LocalId,
                IsAdmin       = blacklist.UserAffected.IsAdmin,
                IsBlacklisted = blacklist.UserAffected.IsBlacklisted,
            },
            ByAdmin = new UserDto
            {
                Id            = admin.Id,
                Username      = admin.Username,
                FirstName     = admin.FirstName,
                LastName      = admin.LastName,
                Email         = admin.Email,
                Local         = admin.LocalId,
                IsAdmin       = admin.IsAdmin,
                IsBlacklisted = admin.IsBlacklisted,
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

    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardDto>> GetDashboardCounts()
    {
        var usersCount            = await context.Users.CountAsync();
        var pendingUsersCount     = await context.PendingUsers.CountAsync();
        var blacklistedUsersCount = await context.BlacklistedUsers.CountAsync();

        return Ok(new DashboardDto
        {
            TotalUsersCount       = usersCount,
            PendingUsersCount     = pendingUsersCount,
            BlacklistedUsersCount = blacklistedUsersCount,
            JobsCount             = 0, //TODO: Fix when implemented
        });
    }
}