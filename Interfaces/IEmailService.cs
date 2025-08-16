using UABackbone_Backend.DTOs;

namespace UABackbone_Backend.Interfaces;

public interface IEmailService
{
    Task<HttpResponseMessage> SendAsync(
        string toEmail, string toName, string subject,
        string? text = null, string? html = null, string? link = null);

    Task<HttpResponseMessage> SendResetLinkAsync(string email, string firstName, string resetLink);
    Task<HttpResponseMessage> SendPendingAsync(string email, string firstName);
    Task<HttpResponseMessage> SendApprovedAsync(string email, string firstName);
}
