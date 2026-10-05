namespace UABackbone_Backend.Models
{
    public class Job
    {
        public int        Id                 { get; set; }
        public int        LocalId            { get; set; }
        public LocalUnion Local              { get; set; } = null!;
        public int        PostedByUserId     { get; set; }
        public User       PostedByUser       { get; set; } = null!;
        public string?    JobName            { get; set; }
        public string?    Contractor         { get; set; }
        public string?    City               { get; set; }
        public string?    State              { get; set; }
        public DateTime?  StartDate          { get; set; }
        public DateTime?  EndDate            { get; set; }
        public string?    Hours              { get; set; }
        public decimal?   PerDiem            { get; set; }
        public string?    Notes              { get; set; }
        public DateTime   PostedAt           { get; set; }
        public DateTime   ExpiresAt          { get; set; }
        public ICollection<JobCraft>       Crafts       { get; set; }
        public ICollection<JobWeldCert>    Certs        { get; set; }
        public ICollection<JobRequirement> Requirements { get; set; }
    }
}
