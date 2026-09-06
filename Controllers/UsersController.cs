using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers;
public class UsersController(RailwayContext context) : BaseApiController
{

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUserByIdAsync(int id)
    {
        var user = await context.Users.FindAsync(id);

        return user != null ? Ok(new UserDto
        {
            Id            = user.Id,
            Username      = user.Username,
            Email         = user.Email,
            FirstName     = user.FirstName,
            LastName      = user.LastName,
            Local         = user.LocalId,
            IsAdmin       = user.IsAdmin,
            IsBlacklisted = user.IsBlacklisted,
        }) : NotFound("User not found");
    }

    [HttpGet("all-users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> GetAllUsersAsync()
    {
        var users    = await context.Users.ToListAsync();
        var userDtos = ConvertToUserDtos(users);

        return Ok(userDtos);
    }

    [HttpGet("paginated-users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<UserDto>>> GetUsersPaginatedAsync(
        int page = 1,
        int limitSize = 25,
        string? searchTerm = null,
        bool? isAdmin = null)
    {
        var users = context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLower();
            users = users.Where(u =>
            u.Id.ToString().Contains(normalized) ||
            u.FirstName.ToLower().Contains(normalized) ||
            u.LastName.ToLower().Contains(normalized) ||
            u.Email.ToLower().Contains(normalized) ||
            u.Username.ToLower().Contains(normalized) ||
            u.LocalId.ToString().Contains(normalized));
        }

        if (isAdmin.HasValue)
        {
            users = users.Where(u => u.IsAdmin == isAdmin.Value);
        }

        var totalCount = await users.CountAsync();

        var allUsers = await users
            .Skip((page - 1) * limitSize)
            .Take(limitSize)
            .ToListAsync();

        var userDtos = ConvertToUserDtos(allUsers);

        return Ok(new PagedResultDto<UserDto>
        {
            Total = totalCount,
            Items = userDtos
        });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUserAsync([FromBody] User aUser, int id)
    {
        aUser.Id = id;
        context.Update(aUser);
        await context.SaveChangesAsync();

        return Ok(new UserDto
        {
            Id        = aUser.Id,
            Username  = aUser.Username,
            Email     = aUser.Email,
            FirstName = aUser.FirstName,
            LastName  = aUser.LastName,
            Local     = aUser.LocalId,
            IsAdmin   = aUser.IsAdmin,
            IsBlacklisted = aUser.IsBlacklisted,
        });
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
                Local = user.LocalId,
                IsAdmin = user.IsAdmin,
                IsBlacklisted = user.IsBlacklisted,
            };
            userDtos.Add(userDto);
        }

        return userDtos;
    }

}