using UABackbone_Backend.Enums;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Services
{
    public class AuditService(RailwayContext context) : IAuditService
    {
        public Task LogActionAsync(
            User admin,
            User? user,
            AuditActionType actionType,
            string? reason = null)
        {
            context.AdminActions.Add(new AdminAction
            {
                ByAdminId      = admin.Id, 
                ByAdmin        = admin,
                UserAffectedId = user.Id,
                UserAffected   = user,
                Action         = FormatAction(actionType),
                Reason         = reason,
                Date           = DateTime.UtcNow
            });

            return Task.CompletedTask;
        }

        private static string FormatAction(AuditActionType actionType)
        {
            return actionType switch
            {
                AuditActionType.UserPromoted      => "Promoted user",
                AuditActionType.UserDemoted       => "Demoted user",
                AuditActionType.UserApproved      => "Approved user",
                AuditActionType.UserRejected      => "Rejected user",
                AuditActionType.UserBlacklisted   => "Blacklisted user",
                AuditActionType.UserUnblacklisted => "Unblacklisted user",
                AuditActionType.UserUpdated       => "Updated user",
                AuditActionType.UserRemoved       => "Removed user",
                AuditActionType.UserAddedBA       => "Added business agent",
                AuditActionType.UserRemovedBA     => "Removed business agent",
                AuditActionType.UserAddedBM       => "Added business manager",
                AuditActionType.UserRemovedBM     => "Removed business manager",
                AuditActionType.LocalRemoved      => "Removed local",
                AuditActionType.LocalAdded        => "Added local",
                AuditActionType.LocalUpdated      => "Updated Local",
                AuditActionType.JobAdded          => "Added job",
                AuditActionType.JobRemoved        => "Removed job",
                AuditActionType.JobUpdated        => "Job updated",
                _                                 => actionType.ToString()
            };
        }
    }
}
