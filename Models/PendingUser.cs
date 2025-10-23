namespace UABackbone_Backend.Models;

public class PendingUser
{
    public int      Id           { get; set; }
    public string   Username     { get; set; }
    public string   PasswordHash { get; set; }
    public string   FirstName    { get; set; }
    public string   LastName     { get; set; }
    public string   Email        { get; set; }
    public int      Local        { get; set; }
    public byte[]   UaCardImage  { get; set; }
    public DateTime SubmittedAt  { get; set; }
}