namespace UABackbone_Backend.Models
{
    public class PendingJob
    {
        public int        Id                 { get; set; }
        public int        LocalId            { get; set; }
        public LocalUnion Local              { get; set; } = null!;
        public int        SubmittedByUserId  { get; set; }
        public User       SubmittedByUser    { get; set; } = null!;
        public string?    JobName            { get; set; }
        public string?    Contractor         { get; set; }
        public string?    City               { get; set; }
        public string?    State              { get; set; }
        public DateTime?  StartDate          { get; set; }
        public DateTime?  EndDate            { get; set; }
        public string?    Hours              { get; set; }
        public decimal?   PerDiem            { get; set; }
        public string?    Notes              { get; set; }
        public DateTime   SubmittedAt        { get; set; }
        public DateTime   ExpiresAt          { get; set; }
        public string     CraftsJson         { get; set; } = "[]";
        public string     CertsJson          { get; set; } = "[]";
        public string     RequirementsJson   { get; set; } = "[]";
        public string?    ProofSource        { get; set; }
        public string?    ProofFileKey       { get; set; }
    }
}
