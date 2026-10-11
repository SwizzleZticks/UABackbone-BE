using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Enums;
using UABackbone_Backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace UABackbone_Backend.Controllers
{
    [Authorize(Policy = "CurrentAdmin")]
    public class PendingJobsController(RailwayContext context) : BaseApiController
    {

        [HttpGet("pending-jobs-paginated")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResultDto<PendingJobDto>>> GetPendingJobsPaginated(
            int page = 1,
            int limitSize = 25,
            string? searchTerm = null)
        {
            page = Math.Max(1, page);
            limitSize = Math.Clamp(limitSize, 1, 100);

            var pendingJobs = context.PendingJobs
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalized = searchTerm.Trim().ToLower();

                pendingJobs = pendingJobs.Where(j =>
                    j.Id.ToString().Contains(normalized) ||
                    j.LocalId.ToString().Contains(normalized) ||
                    j.SubmittedByUserId.ToString().Contains(normalized) ||
                    j.SubmittedByUser.Username.ToLower().Contains(normalized) ||
                    (j.JobName != null &&
                        j.JobName.ToLower().Contains(normalized)) ||
                    (j.Contractor != null &&
                        j.Contractor.ToLower().Contains(normalized)) ||
                    (j.City != null &&
                        j.City.ToLower().Contains(normalized)) ||
                    (j.State != null &&
                        j.State.ToLower().Contains(normalized)));
            }

            var totalCount = await pendingJobs.CountAsync();

            var results = await pendingJobs
                .OrderByDescending(j => j.SubmittedAt)
                .ThenByDescending(j => j.Id)
                .Skip((page - 1) * limitSize)
                .Take(limitSize)
                .Select(j => new
                {
                    Job = j,
                    Username = j.SubmittedByUser.Username
                })
                .ToListAsync();

            var pendingJobDtos = results.Select(result =>
            {
                var job = result.Job;

                return new PendingJobDto
                {
                    Id                  = job.Id,
                    LocalId             = job.LocalId,
                    SubmittedByUserId   = job.SubmittedByUserId,
                    SubmittedByUsername = result.Username,

                    JobName     = job.JobName,
                    Contractor  = job.Contractor,
                    City        = job.City,
                    State       = job.State,
                    StartDate   = job.StartDate,
                    EndDate     = job.EndDate,
                    Hours       = job.Hours,
                    PerDiem     = job.PerDiem,
                    Notes       = job.Notes,
                    ExpiresAt   = job.ExpiresAt,
                    SubmittedAt = job.SubmittedAt,

                    Crafts = System.Text.Json.JsonSerializer.Deserialize<List<CreateJobCraftDto>>(
                        job.CraftsJson) ?? [],

                    Certs = System.Text.Json.JsonSerializer.Deserialize<List<string>>(
                        job.CertsJson) ?? [],

                    Requirements = System.Text.Json.JsonSerializer.Deserialize<List<RequirementType>>(
                        job.RequirementsJson) ?? [],

                    ProofSource = job.ProofSource,
                    HasProofFile = !string.IsNullOrWhiteSpace(job.ProofFileKey)
                };
            }).ToList();

            return Ok(new PagedResultDto<PendingJobDto>
            {
                Total = totalCount,
                Items = pendingJobDtos
            });
        }

    }
}
