using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LocalsController : ControllerBase
{
    private readonly RailwayContext _context;

    public LocalsController(RailwayContext context)
    {
        _context = context;
    }

    [HttpGet("/Locals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LocalUnion>> GetLocalsAsync()
    {
        return Ok(await _context.LocalUnions.ToListAsync());
    }
}