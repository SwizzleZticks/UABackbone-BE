using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers
{
    public class JobsController(RailwayContext context) : BaseApiController
    {

        [HttpGet("all-jobs-paginated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<JobDto>>> GetJobsPaginated(int page = 1, int limitSize = 25, string? searchTerm = null)
        {
            page = Math.Max(1, page);
            limitSize = Math.Clamp(limitSize, 1, 100);

            var jobs = context.Jobs
                .AsNoTracking()
                .Where(j => j.ExpiresAt > DateTime.UtcNow)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalized = searchTerm.Trim().ToLower();

                jobs = jobs.Where(j =>
                    j.Id.ToString().Contains(normalized) ||
                    j.LocalId.ToString().Contains(normalized) ||
                    (j.JobName != null &&
                        j.JobName.ToLower().Contains(normalized)) ||
                    (j.Contractor != null &&
                        j.Contractor.ToLower().Contains(normalized)) ||
                    (j.City != null &&
                        j.City.ToLower().Contains(normalized)) ||
                    (j.State != null &&
                        j.State.ToLower().Contains(normalized)));
            }

            var totalCount = await jobs.CountAsync();

            var allJobs = await jobs
                .OrderByDescending(j => j.PostedAt)
                .ThenByDescending(j => j.Id)
                .Skip((page - 1) * limitSize)
                .Take(limitSize)
                .Select(j => new JobDto
                {
                    Id             = j.Id,
                    LocalId        = j.LocalId,
                    PostedByUserId = j.PostedByUserId,
                    JobName        = j.JobName,
                    Contractor     = j.Contractor,
                    City           = j.City,
                    State          = j.State,
                    StartDate      = j.StartDate,
                    EndDate        = j.EndDate,
                    Hours          = j.Hours,
                    PerDiem        = j.PerDiem,
                    Notes          = j.Notes,
                    PostedAt       = j.PostedAt,
                    ExpiresAt      = j.ExpiresAt,

                    Crafts = j.Crafts.Select(c => new CreateJobCraftDto
                    {
                        Craft = c.Craft,
                        AmountNeeded = c.AmountNeeded
                    }).ToList(),

                    Certs = j.Certs
                        .Select(c => c.Certification)
                        .ToList(),

                    Requirements = j.Requirements
                        .Select(r => r.Requirement)
                        .ToList()
                })
                .ToListAsync();

            return Ok(new PagedResultDto<JobDto>
            {
                Total = totalCount,
                Items = allJobs
            });
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JobDto>> GetJobById(int id)
        {
            var job = await context.Jobs
                .AsNoTracking()
                .Where(j => j.Id == id &&
                            j.ExpiresAt > DateTime.UtcNow)
                .Select(j => new JobDto
                {
                    Id = j.Id,
                    LocalId = j.LocalId,
                    PostedByUserId = j.PostedByUserId,
                    JobName = j.JobName,
                    Contractor = j.Contractor,
                    City = j.City,
                    State = j.State,
                    StartDate = j.StartDate,
                    EndDate = j.EndDate,
                    Hours = j.Hours,
                    PerDiem = j.PerDiem,
                    Notes = j.Notes,
                    PostedAt = j.PostedAt,
                    ExpiresAt = j.ExpiresAt,

                    Crafts = j.Crafts.Select(c => new CreateJobCraftDto
                    {
                        Craft = c.Craft,
                        AmountNeeded = c.AmountNeeded
                    }).ToList(),

                    Certs = j.Certs
                        .Select(c => c.Certification)
                        .ToList(),

                    Requirements = j.Requirements
                        .Select(r => r.Requirement)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (job == null)
                return NotFound();

            return Ok(job);
        }

    }
}
