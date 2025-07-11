namespace UABackbone_Backend.Interfaces;

public interface IEmailService
{
    Task SendResetLink(string toEmail, string resetLink);
}