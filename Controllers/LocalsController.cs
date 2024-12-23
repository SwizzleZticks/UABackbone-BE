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
    
    //TODO
    //Limit number of locals shown
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LocalUnion>> GetLocalsAsync()
    {
        return Ok(await _context.LocalUnions.ToListAsync());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalUnion>> GetLocalByIdAsync(short id)
    {
        var local = await _context.LocalUnions.FindAsync(id);
        return local != null ? Ok(local) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<LocalUnion>> CreateLocalAsync([FromBody] LocalUnion newLocal)
    {
        _context.LocalUnions.Add(newLocal);
        await _context.SaveChangesAsync();

        return newLocal;
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalUnion>> UpdateLocalAsync(short id, [FromBody] LocalUnion aLocal)
    {
        var existingLocal = await _context.LocalUnions.FindAsync(id);

        if (existingLocal == null)
        {
            return NotFound();
        }
        
        existingLocal.Location        = aLocal.Location;
        existingLocal.Wage            = aLocal.Wage;
        existingLocal.Vacation        = aLocal.Vacation;
        existingLocal.HealthWelfare   = aLocal.HealthWelfare;
        existingLocal.NationalPension = aLocal.NationalPension;
        existingLocal.LocalPension    = aLocal.LocalPension;
        existingLocal.Annuity         = aLocal.Annuity;
        existingLocal.LastUpdated     = aLocal.LastUpdated;
        
        await _context.SaveChangesAsync();
        
        return Ok(existingLocal);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteLocalAsync(short id)
    {
        var local = await _context.LocalUnions.FindAsync(id);

        if (local == null)
        {
            return NotFound("Local not found");
        }
        
        _context.LocalUnions.Remove(local);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}