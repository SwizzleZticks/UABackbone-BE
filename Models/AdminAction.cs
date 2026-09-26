namespace UABackbone_Backend.Models
{
    public class AdminAction
    {
        public int             Id             { get; set; }
        public required int    ByAdminId      { get; set; }
        public required User   ByAdmin        { get; set; }
        public required int?   UserAffectedId { get; set; }
        public required User?  UserAffected   { get; set; }
        public required string Action         { get; set; }
        public string?         Reason         { get; set; }
        public DateTime        Date           { get; set; }
    }
}
