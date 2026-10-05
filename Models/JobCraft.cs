namespace UABackbone_Backend.Models
{
    public class JobCraft
    {
        public int    Id           { get; set; }
        public int    JobId        { get; set; }
        public Job    Job          { get; set; } = null!;
        public string Craft        { get; set; } = string.Empty;
        public int    AmountNeeded { get; set; }
    }
}