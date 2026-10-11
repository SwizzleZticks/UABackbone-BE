using Microsoft.AspNetCore.Mvc;

namespace UABackbone_Backend.DTOs
{
    public class PendingJobDto : CreateJobDto
    {
        public int      Id                  { get; set; }
        public int      SubmittedByUserId   { get; set; }
        public string   SubmittedByUsername { get; set; } = string.Empty;
        public DateTime SubmittedAt         { get; set; }
        public string?  ProofSource         { get; set; }
        public bool     HasProofFile        { get; set; }
    }
}
