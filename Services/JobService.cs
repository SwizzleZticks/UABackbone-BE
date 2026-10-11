
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Enums;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Services
{
    public class JobService(RailwayContext context, IAuditService auditService) : IJobService
    {
        public async Task<Job> CreateJobAsync(CreateJobDto dto, User postedBy)
        {
            await ValidateJobAsync(dto);

            if (!postedBy.IsAdmin)
            {
                if ((!postedBy.IsBusinessAgent &&
                     !postedBy.IsBusinessManager) ||
                      postedBy.LocalId != dto.LocalId)
                {
                    throw new UnauthorizedAccessException(
                        "You cannot post jobs for this local.");
                }
            }

            var job = BuildJob(dto, postedBy.Id);

            context.Jobs.Add(job);

            await auditService.LogActionAsync(
                postedBy,
                null,
                AuditActionType.JobAdded);

            await context.SaveChangesAsync();

            return job;
        }

        public async Task<PendingJob> SubmitJobAsync(SubmitJobDto dto, User submittedBy)
        {
            await ValidateJobAsync(dto);

            if (string.IsNullOrWhiteSpace(dto.ProofSource) &&
                string.IsNullOrWhiteSpace(dto.ProofFileKey))
            {
                throw new ArgumentException(
                    "Job submission requires proof.");
            }

            var pendingJob = new PendingJob
            {
                LocalId           = dto.LocalId,
                SubmittedByUserId = submittedBy.Id,
                JobName           = dto.JobName,
                Contractor        = dto.Contractor,
                City              = dto.City,
                State             = dto.State,
                StartDate         = dto.StartDate,
                EndDate           = dto.EndDate,
                Hours             = dto.Hours,
                PerDiem           = dto.PerDiem,
                Notes             = dto.Notes,
                SubmittedAt       = DateTime.UtcNow,
                ExpiresAt         = dto.ExpiresAt,

                CraftsJson       = JsonSerializer.Serialize(dto.Crafts),
                CertsJson        = JsonSerializer.Serialize(dto.Certs),
                RequirementsJson = JsonSerializer.Serialize(dto.Requirements),

                ProofSource  = dto.ProofSource,
                ProofFileKey = dto.ProofFileKey
            };

            context.PendingJobs.Add(pendingJob);

            await context.SaveChangesAsync();

            return pendingJob;
        }

        public async Task<Job?> ApproveJobAsync(int pendingJobId, User admin)
        {
            if (!admin.IsAdmin)
                throw new UnauthorizedAccessException();

            var pendingJob = await context.PendingJobs
                .Include(p => p.SubmittedByUser)
                .FirstOrDefaultAsync(p => p.Id == pendingJobId);

            if (pendingJob == null)
                return null;

            var dto = new CreateJobDto
            {
                LocalId    = pendingJob.LocalId,
                JobName    = pendingJob.JobName,
                Contractor = pendingJob.Contractor,
                City       = pendingJob.City,
                State      = pendingJob.State,
                StartDate  = pendingJob.StartDate,
                EndDate    = pendingJob.EndDate,
                Hours      = pendingJob.Hours,
                PerDiem    = pendingJob.PerDiem,
                Notes      = pendingJob.Notes,
                ExpiresAt  = pendingJob.ExpiresAt,

                Crafts = JsonSerializer.Deserialize<List<CreateJobCraftDto>>(
                    pendingJob.CraftsJson) ?? [],

                Certs = JsonSerializer.Deserialize<List<string>>(
                    pendingJob.CertsJson) ?? [],

                Requirements = JsonSerializer.Deserialize<List<RequirementType>>(
                    pendingJob.RequirementsJson) ?? []
            };

            await ValidateJobAsync(dto);

            var job = BuildJob(dto, pendingJob.SubmittedByUserId);

            context.Jobs.Add(job);
            context.PendingJobs.Remove(pendingJob);

            await auditService.LogActionAsync(
                admin,
                pendingJob.SubmittedByUser,
                AuditActionType.JobApproved);

            await context.SaveChangesAsync();

            return job;
        }

        public async Task<bool> RejectJobAsync(int pendingJobId, User admin, string reason)
        {
            if (!admin.IsAdmin)
                throw new UnauthorizedAccessException();

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException(
                    "Rejection reason is required.");

            var pendingJob = await context.PendingJobs
                .Include(p => p.SubmittedByUser)
                .FirstOrDefaultAsync(p => p.Id == pendingJobId);

            if (pendingJob == null)
                return false;

            context.PendingJobs.Remove(pendingJob);

            await auditService.LogActionAsync(
                admin,
                pendingJob.SubmittedByUser,
                AuditActionType.JobRejected,
                reason);

            await context.SaveChangesAsync();

            return true;
        }

        private async Task ValidateJobAsync(CreateJobDto dto)
        {
            if (!await context.LocalUnions
                .AnyAsync(l => l.Local == dto.LocalId))
            {
                throw new ArgumentException(
                    "Invalid local union.");
            }

            if (dto.ExpiresAt <= DateTime.UtcNow)
                throw new ArgumentException(
                    "Expiration date must be in the future.");

            if (dto.StartDate.HasValue &&
                dto.EndDate.HasValue &&
                dto.EndDate < dto.StartDate)
            {
                throw new ArgumentException(
                    "End date cannot be before start date.");
            }

            if (dto.PerDiem < 0)
                throw new ArgumentException(
                    "Per diem cannot be negative.");

            if (dto.Crafts.Any(c =>
                string.IsNullOrWhiteSpace(c.Craft) ||
                c.AmountNeeded <= 0))
            {
                throw new ArgumentException(
                    "Invalid craft information.");
            }

            if (dto.Certs.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException(
                    "Invalid certification.");
        }

        private static Job BuildJob(
            CreateJobDto dto,
            int postedByUserId)
        {
            return new Job
            {
                LocalId        = dto.LocalId,
                PostedByUserId = postedByUserId,
                JobName        = dto.JobName,
                Contractor     = dto.Contractor,
                City           = dto.City,
                State          = dto.State,
                StartDate      = dto.StartDate,
                EndDate        = dto.EndDate,
                Hours          = dto.Hours,
                PerDiem        = dto.PerDiem,
                Notes          = dto.Notes,
                PostedAt       = DateTime.UtcNow,
                ExpiresAt      = dto.ExpiresAt,

                Crafts = dto.Crafts.Select(c => new JobCraft
                {
                    Craft = c.Craft,
                    AmountNeeded = c.AmountNeeded
                }).ToList(),

                Certs = dto.Certs.Select(c => new JobWeldCert
                {
                    Certification = c
                }).ToList(),

                Requirements = dto.Requirements.Select(r =>
                    new JobRequirement
                    {
                        Requirement = r
                    }).ToList()
            };
        }
    }
}

