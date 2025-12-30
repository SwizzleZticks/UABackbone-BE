namespace UABackbone_Backend.Models;

public class PendingUser
{
    public int               Id           { get; set; }
    public required string   Username     { get; set; }
    public required string   PasswordHash { get; set; }
    public required string   FirstName    { get; set; }
    public required string   LastName     { get; set; }
    public required string   Email        { get; set; }
    public int               Local        { get; set; }
    public required byte[]   UaCardImage  { get; set; }
    public DateTime          SubmittedAt  { get; set; }
}