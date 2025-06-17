using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers;
public class UserController(RailwayContext context) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersPaginatedAsync(int page = 1, int limitSize = 25)
    {
        var users = await context.Users
            .Skip((page - 1) * limitSize)
            .Take(limitSize)
            .ToListAsync();
        List<UserDto> userDtos = new List<UserDto>();

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
        return Ok(userDtos);
    }
    
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> GetAllUsersAsync()
    {
        var users = await context.Users.ToListAsync();
        List<UserDto> userDtos = new List<UserDto>();

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

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUserAsync([FromBody] User aUser, ushort id)
    {
        aUser.Id = id;
        context.Update(aUser);
        await context.SaveChangesAsync();

        return Ok(new UserDto
        {
            Id = aUser.Id,
            Username = aUser.Username,
            Email = aUser.Email,
            FirstName = aUser.FirstName,
            LastName = aUser.LastName,
            Local = aUser.LocalId
        });
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