using UABackbone_Backend.Enums;

namespace UABackbone_Backend.Models
{
    public class JobRequirement
    {
        public int             Id          { get; set; }
        public int             JobId       { get; set; }
        public Job             Job         { get; set; } = null!;
        public RequirementType Requirement { get; set; }
    }
}
