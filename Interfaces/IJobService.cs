using UABackbone_Backend.DTOs;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Interfaces
{
    public interface IJobService
    {
        // Direct posting by authorized users
        Task<Job> CreateJobAsync(CreateJobDto dto, User postedBy);

        // Member submissions
        Task<PendingJob> SubmitJobAsync(SubmitJobDto dto, User submittedBy);

        // Admin review
        Task<Job?> ApproveJobAsync(int pendingJobId, User admin);
        Task<bool> RejectJobAsync(int pendingJobId, User admin, string reason);
    }
}
