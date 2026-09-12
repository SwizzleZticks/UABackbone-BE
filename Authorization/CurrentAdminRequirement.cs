using Microsoft.AspNetCore.Authorization;

namespace UABackbone_Backend.Authorization
{
    public record CurrentAdminRequirement : IAuthorizationRequirement
    {

    }
}
