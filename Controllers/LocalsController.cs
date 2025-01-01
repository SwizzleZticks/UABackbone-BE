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

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalUnion>> GetLocalByIdAsync(short id)
    {
        var local = await context.LocalUnions.FindAsync(id);
        return local != null ? Ok(local) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<LocalUnion>> CreateLocalAsync([FromBody] LocalUnion newLocal)
    {
        context.LocalUnions.Add(newLocal);
        await context.SaveChangesAsync();

        return newLocal;
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalUnion>> UpdateLocalAsync(short id, [FromBody] LocalUnion aLocal)
    {
        aLocal.Local = id;
        context.Update(aLocal);
        await context.SaveChangesAsync();
        
        return Ok(aLocal);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteLocalAsync(short id)
    {
        var local = await context.LocalUnions.FindAsync(id);

        if (local == null)
        {
            return NotFound("Local not found");
        }
        
        context.LocalUnions.Remove(local);
        await context.SaveChangesAsync();
        
        return NoContent();
    }
}