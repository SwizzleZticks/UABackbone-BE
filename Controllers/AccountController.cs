using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers;

public class AccountController(RailwayContext context, IEmailService emailService ,ITokenService tokenService) : BaseApiController
{
    [HttpPost("verify/{id}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<UserDto>> VerifyUserAsync([FromBody]ushort id)
    {
        var pendingUser = await context.PendingUsers.FindAsync(id);
        
        var user = new User
        {
            Username = pendingUser.Username.ToLower(),
            PasswordHash = pendingUser.PasswordHash,
            FirstName = pendingUser.FirstName,
            LastName = pendingUser.LastName,
            Email = pendingUser.Email,
            IsVerified = true,
            IsAdmin = false,
            IsBlacklisted = false,
            LocalId = pendingUser.Local
        };
        
        context.PendingUsers.Remove(pendingUser);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Local = user.LocalId,
            Token = tokenService.CreateToken(user)
        };

        return Created("api/Account/verify",userDto);
    }
    
    [HttpPost("register-pending")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<PendingUser>> PendingCreationAsync([FromForm]RegisterDto dto, IFormFile uaCard)
    {
         
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        if (await UserExistsAsync(dto.Username))
        {
            return BadRequest("Username is taken");
        }

        if (await EmailExistsAsync(dto.Email))
        {
            return BadRequest("Email is taken");
        }
        
        using var ms = new MemoryStream();
        await uaCard.CopyToAsync(ms);
        
        var pendingUser = new PendingUser()
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Local = dto.Local,
            UaCardImage = ms.ToArray(),
            SubmittedAt = DateTime.Now
        };
        
        context.PendingUsers.Add(pendingUser);
        await context.SaveChangesAsync();

        var userDto = new UserDto()
        {
            Id = pendingUser.Id,
            Username = pendingUser.Username,
            FirstName = pendingUser.FirstName,
            LastName = pendingUser.LastName,
            Email = pendingUser.Email,
            Local = pendingUser.Local,
        };

        return Created("api/Account/register-pending", userDto);
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> LoginAsync([FromBody] LoginDto loginDto)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

        if (user == null)
        {
            return Unauthorized("Email does not exist");
        }

        var password = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

        if (!password)
        {
            return Unauthorized("Invalid password");
        }
        
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Local = user.LocalId,
            Token = tokenService.CreateToken(user)
        };
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordDto dto)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

        if (user == null)
        {
            return BadRequest("Email does not exist");
        }
        
        var token = tokenService.CreateToken(user, 1, "reset");
        
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
        
        await context.SaveChangesAsync();
        
        var resetLink = $"http://localhost:4200/reset-password?token={token}";
        
        await emailService.SendResetLinkAsync(user.Email, user.FirstName, resetLink);
        
        return Ok(new { message = "Password reset link sent."});
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto dto)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token);

        if (user == null || user.PasswordResetTokenExpires < DateTime.UtcNow)
        {
            return BadRequest("No user found or reset token has expired");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpires = null;
        await context.SaveChangesAsync();
        
        return Ok(new  { message = "Password has been successfully reset."});
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
    
    private async Task<bool> UserExistsAsync(string username)
    {
        return await context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()) ||
               await context.PendingUsers.AnyAsync(p => p.Email.ToLower() == username.ToLower());
    }
    
    private async Task<bool> EmailExistsAsync(string email)
    {
        return await context.Users.AnyAsync(u => u.Email == email) ||
               await context.PendingUsers.AnyAsync(p => p.Email == email);;
    }
}