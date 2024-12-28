using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : Controller
{
    private readonly RailwayContext _context;

    public UserController(RailwayContext context)
    {
        _context = context;
    }
    
    //Get all users
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LocalUnion>> GetUsersAsync()
    {
        return Ok(await _context.Users.ToListAsync());
    }
    //Get user by ID
    
    //Update user
    
    //Create user
    
    //Delete user
}