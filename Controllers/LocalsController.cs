using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers;
public class LocalsController(RailwayContext context) : BaseApiController
{
    //TODO
    //Limit number of locals shown(possible start index and end index query)-list.getrange
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

    [HttpPut("{local}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalUnion>> UpdateLocalAsync(short local, [FromBody] LocalUnion aLocal)
    {
        aLocal.Local = local;
        context.Update(aLocal);
        await context.SaveChangesAsync();
        
        return Ok(aLocal);
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