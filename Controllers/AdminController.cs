using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers;

public class AdminController(RailwayContext context, IEmailService emailService, ITokenService tokenService) : BaseApiController
{
    [HttpPost("verify/{id}/approve")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<UserDto>> VerifyUserAsync(ushort id)
    {
        var pendingUser = await context.PendingUsers.FindAsync(id);
        if (pendingUser == null) return NotFound();

        var user = new User
        {
            Username = pendingUser.Username.Trim().ToLowerInvariant(),
            PasswordHash = pendingUser.PasswordHash,
            FirstName = pendingUser.FirstName?.Trim(),
            LastName = pendingUser.LastName?.Trim(),
            Email = pendingUser.Email.Trim().ToLowerInvariant(),
            IsVerified = true,
            IsAdmin = false,
            IsBlacklisted = false,
            LocalId = pendingUser.Local
        };

        context.PendingUsers.Remove(pendingUser);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        await emailService.SendApprovedAsync(user.Email, user.FirstName ?? "");

        var userDto = new UserDto
        {
            Id        = user.Id,
            Username  = user.Username,
            FirstName = user.FirstName,
            LastName  = user.LastName,
            Email     = user.Email,
            Local     = user.LocalId,
            Token     = tokenService.CreateToken(user)
        };

        return Created("api/Account/verify", userDto);
    }
    
    [HttpGet("pending-users/{id}/uacard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUaCardAsync(ushort id)
    {
        var user =  await context.PendingUsers.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }
        
        return File(user.UaCardImage, "image/jpeg");
    }
    
    [HttpDelete("pending-users/{id}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> DeletePendingUserAsync(ushort id)
    {
        var user = await context.PendingUsers.FindAsync(id);

        if (user == null)
        {
            return NotFound("User not found");
        }
        context.PendingUsers.Remove(user);
        await context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersPaginatedAsync(int page = 1, int limitSize = 25)
    {
        var users = await context.Users
            .Skip((page - 1) * limitSize)
            .Take(limitSize)
            .ToListAsync();
        var userDtos = ConvertToUserDtos(users);

        return Ok(userDtos);
    }

    [HttpGet("all-pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PendingUser>> GetAllPendingUsersAsync()
    {
        var pendingUsers = await context.PendingUsers.ToListAsync();
        var pendingUserDtos = (from pendingUser in pendingUsers
                               let pendingUserDto = new UserDto
                               {
                                   Id = pendingUser.Id,
                                   Username = pendingUser.Username,
                                   Email = pendingUser.Email,
                                   FirstName = pendingUser.FirstName,
                                   LastName = pendingUser.LastName,
                                   Local = pendingUser.Local
                               }
                               select pendingUser).ToList();

        return Ok(pendingUserDtos);
    }

    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> GetAllUsersAsync()
    {
        var users = await context.Users.ToListAsync();
        var userDtos = ConvertToUserDtos(users);

        return Ok(userDtos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUserByIdAsync(ushort id)
    {
        var user = await context.Users.FindAsync(id);

        return user != null ? Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Local = user.LocalId
        }) : NotFound("User not found");
    }

    private List<UserDto> ConvertToUserDtos(List<User> users)
    {
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Local = user.LocalId
            };
            userDtos.Add(userDto);
        }

        return userDtos;
    }


    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> DeleteUserAsync(ushort id)
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
}