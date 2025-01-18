using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.Models;
using UABackbone_Backend.DTOs;

namespace UABackbone_Backend.Controllers;
[ApiController]
[Route("api/[controller]")]
public class LocalsController(RailwayContext context) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LocalUnion>> GetLocalsAsync()
    {
        return Ok(await context.LocalUnions.ToListAsync());
    }

    [HttpGet("{local}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalUnion>> GetLocalByIdAsync(short local)
    {
        var localUnion = await context.LocalUnions.FindAsync(local);
        return localUnion != null ? Ok(localUnion) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LocalUnion>> CreateLocalAsync([FromBody] LocalUnion newLocal)
    {
        context.LocalUnions.Add(newLocal);
        await context.SaveChangesAsync();

        return Created("api/Locals",newLocal);
    }

    [HttpPatch("{local}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalUnion>> UpdateLocalAsync(short local, [FromBody] LocalUnionDto aLocal)
    {
        var queriedLocal = await context.LocalUnions.FindAsync(local);
        if (queriedLocal == null)
        {
            return NotFound("Local not found");
        }
        
        var dtoProperties = typeof(LocalUnionDto).GetProperties();
        foreach (var prop in dtoProperties)
        {
            var value = prop.GetValue(aLocal);
            if (value != null)
            {
                var entityProperty = queriedLocal.GetType().GetProperty(prop.Name);
                if (entityProperty != null)
                {
                    entityProperty.SetValue(queriedLocal, value);
                }
            }
        }
        await context.SaveChangesAsync();

        return Ok(queriedLocal);
    }

    [HttpDelete("{local}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteLocalAsync(short local)
    {
        var localUnion = await context.LocalUnions.FindAsync(local);

        if (localUnion == null)
        {
            return NotFound("Local not found");
        }
        
        context.LocalUnions.Remove(localUnion);
        await context.SaveChangesAsync();
        
        return NoContent();
    }
}