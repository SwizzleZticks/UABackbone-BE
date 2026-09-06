namespace UABackbone_Backend.DTOs
{
    public class DashboardDto
    {
        public int TotalUsersCount { get; set; }
        public int PendingUsersCount { get; set; }
        public int BlacklistedUsersCount { get; set; }
        public int JobsCount { get; set; } //TODO: Fix when implementing job table
    }
}
