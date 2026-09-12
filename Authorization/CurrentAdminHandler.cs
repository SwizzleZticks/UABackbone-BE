using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Authorization
{
    public class CurrentAdminHandler(RailwayContext railwayContext) : AuthorizationHandler<CurrentAdminRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CurrentAdminRequirement requirement)
        {
            var sidClaim = context.User.FindFirst(c => c.Type == ClaimTypes.Sid);

            if (sidClaim == null)
            {
                return;
            }

            var isValidUserId = int.TryParse(sidClaim.Value, out int id);

            if (!isValidUserId)
            {
                return;
            }

            var user = await railwayContext.Users.FindAsync(id);

            if (user == null)
            {
                return;
            }

            if (user.IsAdmin)
            {
                context.Succeed(requirement);
            }
        }
    }
}
