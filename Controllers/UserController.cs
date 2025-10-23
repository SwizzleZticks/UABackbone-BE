using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers;
public class UserController(RailwayContext context, IIdentityService identityService) : BaseApiController
{
   

    [HttpGet("check-username")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckUsernameAsync([FromQuery]string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest("Username field is required");
        }

        bool isTaken = await identityService.UsernameExistsAsync(username);

        
        return isTaken ? Ok() : NotFound();
    }

    [HttpGet("check-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckEmailAsync([FromQuery]string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email field is required");
        }

        bool isTaken = await identityService.EmailExistsAsync(email);

        return isTaken ? Ok() : NotFound();
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

}