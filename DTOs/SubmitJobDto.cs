namespace UABackbone_Backend.DTOs
{
    public class SubmitJobDto : CreateJobDto
    {
        public string? ProofSource  { get; set; }
        public string? ProofFileKey { get; set; }
    }
}
