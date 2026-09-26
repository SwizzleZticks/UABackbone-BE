using UABackbone_Backend.Enums;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(
            User admin,
            User? user,
            AuditActionType actionType,
            string? reason = null);
    }
}
