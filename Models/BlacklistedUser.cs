using System.ComponentModel.DataAnnotations;

namespace UABackbone_Backend.Models
{
    public class BlacklistedUser
    {
        public int               Id           { get; set; }
        public required User     UserAffected { get; set; }
        public required User     ByAdmin      { get; set; }
        [MinLength(1)]
        [MaxLength(500)]
        public required string   Reason       { get; set; }
        public required DateTime Date         { get; set; }
    }
}
