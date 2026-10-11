
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers
{
    [Authorize]
    public class JobSubmissionsController(RailwayContext context, IJobService jobService) : BaseApiController
    {
        [HttpPost("submit")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SubmitJobAsync(
            [FromBody] SubmitJobDto dto)
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

            if (!user.IsVerified || user.IsBlacklisted)
                return Forbid();

            try
            {
                var pendingJob = await jobService.SubmitJobAsync(dto, user);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    pendingJob.Id,
                    Message = "Job submitted for admin review."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
