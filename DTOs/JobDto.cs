using UABackbone_Backend.Enums;

namespace UABackbone_Backend.DTOs
{
    public class JobDto
    {
        public int       Id             { get; set; }
        public int       LocalId        { get; set; }
        public int       PostedByUserId { get; set; }
        public string?   JobName        { get; set; }
        public string?   Contractor     { get; set; }
        public string?   City           { get; set; }
        public string?   State          { get; set; }
        public DateTime? StartDate      { get; set; }
        public DateTime? EndDate        { get; set; }
        public string?   Hours          { get; set; }
        public decimal?  PerDiem        { get; set; }
        public string?   Notes          { get; set; }
        public DateTime  PostedAt       { get; set; }
        public DateTime  ExpiresAt      { get; set; }
        public List<CreateJobCraftDto> Crafts       { get; set; } = [];
        public List<string>            Certs        { get; set; } = [];
        public List<RequirementType>   Requirements { get; set; } = [];
    }
}
