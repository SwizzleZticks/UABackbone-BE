using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers;

public class AccountController(RailwayContext context, IEmailService emailService ,ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<UserDto>> RegisterAsync([FromBody]RegisterDto registerDto)
    {
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        if (await UserExistsAsync(registerDto.Username))
        {
            return BadRequest("Username is taken");
        }

        if (await EmailExistsAsync(registerDto.Email))
        {
            return BadRequest("Email is taken");
        }
        
        
        var user = new User
        {
            Username = registerDto.Username.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            IsVerified = false,
            IsAdmin = false,
            IsBlacklisted = false,
            LocalId = registerDto.Local
        };
    
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = registerDto.Username,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            Local = registerDto.Local,
            Token = tokenService.CreateToken(user)
        };

        return Created("api/Account/register",userDto);
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
        
        var resetLink = $"https://localhost:4200/auth/reset-password?token={token}";
        
        await emailService.SendResetLinkAsync(user.Email, user.FirstName, resetLink);
        
        return Ok(new { message = "Password reset link sent."});
    }
    
    private async Task<bool> UserExistsAsync(string username)
    {
        return await context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }
    
    private async Task<bool> EmailExistsAsync(string email)
    {
        return await context.Users.AnyAsync(u => u.Email == email);
    }
}