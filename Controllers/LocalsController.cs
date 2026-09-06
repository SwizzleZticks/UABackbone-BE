using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.Models;
using UABackbone_Backend.DTOs;

namespace UABackbone_Backend.Controllers;
public class LocalsController(RailwayContext context) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<LocalUnion>>> GetLocalPaginationAsync(
        int page = 1,
        int limitSize = 25,
        string? searchTerm = null)
    {
        var query = context.LocalUnions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim().ToLower();

            query = query.Where(l =>
                l.Local.ToString().Contains(normalized) ||
                l.Location.ToLower().Contains(normalized));
        }

        var totalCount = await query.CountAsync();

        var locals = await query
            .Skip((page - 1) * limitSize)
            .Take(limitSize)
            .ToListAsync();

        return Ok(new PagedResultDto<LocalUnion>
        {
            Total = totalCount,
            Items = locals
        });
    }
    
    [HttpGet("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LocalUnion>> GetLocalsAsync()
    {
        return Ok(await context.LocalUnions.ToListAsync());
    }

    [HttpGet("register-options")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegisterOptionsAsync()
    {
        var locals = await context.LocalUnions.Select(u => new
        {
            u.Local, u.Location
        }).ToListAsync();
        
        return Ok(locals);
    }
    

    [HttpGet("{local}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalUnion>> GetLocalByIdAsync(int local)
    {
        var localUnion = await context.LocalUnions.FindAsync(local);
        return localUnion != null ? Ok(localUnion) : NotFound();
    }
}