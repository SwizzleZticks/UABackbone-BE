namespace UABackbone_Backend.Interfaces;

public interface IEmailService
{
    Task<HttpResponseMessage> SendResetLinkAsync(string email, string name, string resetLink);
}