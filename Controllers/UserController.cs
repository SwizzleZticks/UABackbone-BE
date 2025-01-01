using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers;
public class UserController(RailwayContext context) : BaseApiController
{
    //Get all users
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LocalUnion>> GetUsersAsync()
    {
        return Ok(await context.Users.ToListAsync());
    }
    //Get user by ID
    
    //Update user
    
    //Create user
    
    //Delete user
}