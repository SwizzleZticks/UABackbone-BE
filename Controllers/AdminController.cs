using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers;
[Authorize(Policy = "CurrentAdmin")]
public class AdminController(
    RailwayContext context, 
    IEmailService emailService, 
    ITokenService tokenService, 
    IUserMapper userMapperService,
    IAuditService auditService)
    : BaseApiController
{
    [HttpPut("user/update/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUserAsync([FromBody] User aUser, int id)
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

        user.Username  = aUser.Username;
        user.Email     = aUser.Email;
        user.FirstName = aUser.FirstName;
        user.LastName  = aUser.LastName;
        user.LocalId   = aUser.LocalId;

        await auditService.LogActionAsync(admin, user, Enums.AuditActionType.UserUpdated);
        await context.SaveChangesAsync();

        return Ok(userMapperService.ToUserDto(user));
    }

    [HttpPost("user/add-blacklist/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BlacklistUserAsync(int id, [FromBody] string reason)
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

        if (string.IsNullOrWhiteSpace(reason))
        {
            return BadRequest("A blacklist reason is required.");
        }

        user.IsBlacklisted = true;
        user.IsAdmin = false;
        user.IsBusinessAgent = false;
        user.IsBusinessManager = false;

        await auditService.LogActionAsync(admin, user, Enums.AuditActionType.UserBlacklisted, reason.Trim());

        context.BlacklistedUsers.Add(new BlacklistedUser
        {
            UserAffected = user,
            ByAdmin      = admin,
            Reason       = reason.Trim(),
            Date         = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        return Ok(userMapperService.ToUserDto(user));
    }

    [HttpDelete("user/remove-blacklist/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBlacklistedUser(int id)
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

        var blacklistEntry = await context.BlacklistedUsers
            .Include(b => b.UserAffected)
            .FirstOrDefaultAsync(b => b.UserAffected.Id == id);

        if (blacklistEntry is null)
        {
            return NotFound("Blacklist entry not found.");
        }

        var user = blacklistEntry.UserAffected;

        if (user is null)
        {
            return NotFound("User not found.");
        }

        user.IsBlacklisted = false;

        context.BlacklistedUsers.Remove(blacklistEntry);

        await auditService.LogActionAsync(
            admin,
            user,
            Enums.AuditActionType.UserUnblacklisted);

        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("user/admin-toggle/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ToggleAdminAsync(int id)
    {
        var sidClaim = User.FindFirst(c => c.Type == ClaimTypes.Sid);

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

        if (user == null)
        {
            return NotFound("User not found");
        }

        if (user.IsBlacklisted)
        {
            return Conflict("Cannot change admin status for a blacklisted user.");
        }


        user.IsAdmin = !user.IsAdmin;

        await auditService.LogActionAsync(
            admin,
            user,
            user.IsAdmin 
            ? Enums.AuditActionType.UserPromoted 
            : Enums.AuditActionType.UserDemoted);

        await context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("pending-user/approve/{id}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> VerifyUserAsync(int id)
    {
        var sidClaim = User.FindFirst(c => c.Type == ClaimTypes.Sid);

        if (sidClaim is null || !int.TryParse(sidClaim.Value, out var adminId))
        {
            return Unauthorized("Missing or invalid admin identity.");
        }

        var admin = await context.Users.FindAsync(adminId);

        if (admin is null)
        {
            return NotFound("Admin not found.");
        }

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

        await auditService.LogActionAsync(
          admin,
          user,
          Enums.AuditActionType.UserApproved);

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
    
    [HttpPost("pending-user/reject/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> DeletePendingUserAsync(int id, [FromBody]string reason)
    {
        var sidClaim = User.FindFirst(c => c.Type == ClaimTypes.Sid);

        if (sidClaim is null || !int.TryParse(sidClaim.Value, out var adminId))
        {
            return Unauthorized("Missing or invalid admin identity.");
        }

        var admin = await context.Users.FindAsync(adminId);

        if (admin is null)
        {
            return NotFound("Admin not found.");
        }

        var user = await context.PendingUsers.FindAsync(id);

        if (user == null)
        {
            return NotFound("User not found");
        }

        await auditService.LogActionAsync(
         admin,
         null,
         Enums.AuditActionType.UserRejected,
         $"User: {user.FirstName} {user.LastName} ({user.Email}), Reason: {reason}");

        context.PendingUsers.Remove(user);
        await context.SaveChangesAsync();

        await emailService.SendDeniedAsync(user.Email, user.FirstName, reason);
        
        return NoContent();
    }

    [HttpPost("local")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LocalUnion>> CreateLocalAsync([FromBody] LocalUnion newLocal)
    {
        var sidClaim = User.FindFirst(c => c.Type == ClaimTypes.Sid);

        if (sidClaim is null || !int.TryParse(sidClaim.Value, out var adminId))
        {
            return Unauthorized("Missing or invalid admin identity.");
        }

        var admin = await context.Users.FindAsync(adminId);

        if (admin is null)
        {
            return NotFound("Admin not found.");
        }

        await auditService.LogActionAsync(admin, null, Enums.AuditActionType.LocalAdded, $"Local {newLocal.Local}");
        context.LocalUnions.Add(newLocal);
        await context.SaveChangesAsync();

        return Created("api/Locals", newLocal);
    }


    [HttpPatch("local/{local}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalUnion>> UpdateLocalAsync(int local, [FromBody] LocalUnionDto aLocal)
    {
        var sidClaim = User.FindFirst(c => c.Type == ClaimTypes.Sid);

        if (sidClaim is null || !int.TryParse(sidClaim.Value, out var adminId))
        {
            return Unauthorized("Missing or invalid admin identity.");
        }

        var admin = await context.Users.FindAsync(adminId);

        if (admin is null)
        {
            return NotFound("Admin not found.");
        }

        var queriedLocal = await context.LocalUnions.FindAsync(local);
        if (queriedLocal == null)
        {
            return NotFound("Local not found");
        }

        var dtoProperties = typeof(LocalUnionDto).GetProperties();
        foreach (var prop in dtoProperties)
        {
            var value = prop.GetValue(aLocal);
            if (value != null)
            {
                var entityProperty = queriedLocal.GetType().GetProperty(prop.Name);
                if (entityProperty != null)
                {
                    entityProperty.SetValue(queriedLocal, value);
                }
            }
        }

        await auditService.LogActionAsync(admin, null, Enums.AuditActionType.LocalUpdated, $"Local {queriedLocal.Local}");
        await context.SaveChangesAsync();

        return Ok(queriedLocal);
    }

    [HttpDelete("local/{local}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteLocalAsync(int local)
    {
        var sidClaim = User.FindFirst(c => c.Type == ClaimTypes.Sid);

        if (sidClaim is null || !int.TryParse(sidClaim.Value, out var adminId))
        {
            return Unauthorized("Missing or invalid admin identity.");
        }

        var admin = await context.Users.FindAsync(adminId);

        if (admin is null)
        {
            return NotFound("Admin not found.");
        }

        var localUnion = await context.LocalUnions.FindAsync(local);

        if (localUnion == null)
        {
            return NotFound("Local not found");
        }

        await auditService.LogActionAsync(admin, null, Enums.AuditActionType.LocalUpdated, local.ToString());
        context.LocalUnions.Remove(localUnion);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("dashboard-data")]
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