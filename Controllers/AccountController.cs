using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers;

public class AccountController(RailwayContext context) : BaseApiController
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<User>> RegisterAsync([FromBody]RegisterDto registerDto)
    {
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.PasswordHash),
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

        return Created();
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