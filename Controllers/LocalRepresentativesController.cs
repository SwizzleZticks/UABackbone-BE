using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers
{
    [Authorize]
    public class LocalRepresentativesController(RailwayContext context, IJobService jobService) : BaseApiController
    {

        [HttpPost("job/create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateJobAsync([FromBody] CreateJobDto dto)
        {
            var sidClaim = User.FindFirst(c => c.Type == ClaimTypes.Sid);

            if (sidClaim is null ||
                !int.TryParse(sidClaim.Value, out var userId))
            {
                return Unauthorized("Missing or invalid user identity.");
            }

            var user = await context.Users.FindAsync(userId);

            if (user is null)
                return Unauthorized("User not found.");

            if (!user.IsVerified ||
                user.IsBlacklisted ||
                (!user.IsBusinessAgent && !user.IsBusinessManager))
            {
                return Forbid();
            }

            try
            {
                var job = await jobService.CreateJobAsync(dto, user);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    job.Id,
                    Message = "Job created successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }


        [HttpPut("job/update/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateJobAsync(int id, [FromBody] CreateJobDto dto)
        {
            var sidClaim = User.FindFirst(c => c.Type == ClaimTypes.Sid);

            if (sidClaim is null ||
                !int.TryParse(sidClaim.Value, out var userId))
            {
                return Unauthorized("Missing or invalid user identity.");
            }

            var user = await context.Users.FindAsync(userId);

            if (user is null)
                return Unauthorized("User not found.");

            if (!user.IsVerified ||
                user.IsBlacklisted ||
                (!user.IsBusinessAgent && !user.IsBusinessManager))
            {
                return Forbid();
            }

            try
            {
                var job = await jobService.UpdateJobAsync(id, dto, user);

                if (job is null)
                    return NotFound("Job not found.");

                return Ok(new
                {
                    job.Id,
                    Message = "Job updated successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpDelete("job/delete/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteJobAsync(int id)
        {
            var sidClaim = User.FindFirst(c => c.Type == ClaimTypes.Sid);

            if (sidClaim is null ||
                !int.TryParse(sidClaim.Value, out var userId))
            {
                return Unauthorized("Missing or invalid user identity.");
            }

            var user = await context.Users.FindAsync(userId);

            if (user is null)
                return Unauthorized("User not found.");

            if (!user.IsVerified ||
                user.IsBlacklisted ||
                (!user.IsBusinessAgent && !user.IsBusinessManager))
            {
                return Forbid();
            }

            try
            {
                var deleted = await jobService.DeleteJobAsync(id, user);

                if (!deleted)
                    return NotFound("Job not found.");

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
