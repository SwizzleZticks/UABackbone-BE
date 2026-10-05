namespace UABackbone_Backend.DTOs
{
    public class CreateJobCraftDto
    {
        public string Craft        { get; set; } = string.Empty;
        public int    AmountNeeded { get; set; }
    }
}
