namespace UABackbone_Backend.Models
{
    public class JobWeldCert
    {
        public int    Id            { get; set; }
        public int    JobId         { get; set; }
        public Job    Job           { get; set; } = null!;
        public string Certification { get; set; } = string.Empty;
    }
}
