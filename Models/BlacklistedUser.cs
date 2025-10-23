namespace UABackbone_Backend.Models
{
    public class BlacklistedUser
    {
        public int               Id           { get; set; }
        public required User     UserAffected { get; set; }
        public required User     ByAdmin      { get; set; }
        public required string   Reason       { get; set; }
        public required DateTime Date         { get; set; }
    }
}
