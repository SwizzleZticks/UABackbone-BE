using System.Text;
using System.Text.Json;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Interfaces;

namespace UABackbone_Backend.Services;
public class EmailService(HttpClient client) : IEmailService
{
    public async Task<HttpResponseMessage> SendAsync(
        string toEmail,
        string toName,
        string subject,
        string? text = null,
        string? html = null,
        string? link = null)
    {
        if (string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(link))
            text = $"{subject}\n{link}";

        if (string.IsNullOrWhiteSpace(html) && !string.IsNullOrWhiteSpace(link))
            html = $"<p>{subject} — <a href=\"{link}\">Open</a></p>";
        
        text ??= subject;
        html ??= $"<p>{subject}</p>";

        return await ComposeAndSendAsync(toEmail, toName, subject, text, html);
    }

    private async Task<HttpResponseMessage> ComposeAndSendAsync(
    string toEmail, string toName, string subject, string text, string html)
    {
        var emailDto = new SendEmailDto
        {
            From = new EmailContactDto { Email = GetFromEmail(), Name = GetFromName() },
            To = new List<EmailContactDto> {
            new EmailContactDto { Email = toEmail, Name = toName ?? string.Empty } // <- guard null
        },
            Subject = subject,
            Text = text,
            Html = html
        };

        var json = JsonSerializer.Serialize(emailDto); // attributes already handle casing
        var resp = await client.PostAsync("email", new StringContent(json, Encoding.UTF8, "application/json"));

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"MAILERSEND FAIL {(int)resp.StatusCode}: {body}");
        }
        return resp;
    }

    static string GetFromEmail() => Environment.GetEnvironmentVariable("MAIL_FROM") ?? "noreply@uabackbone.com";
    static string GetFromName() => Environment.GetEnvironmentVariable("MAIL_FROM_NAME") ?? "UA Backbone";


    public Task<HttpResponseMessage> SendResetLinkAsync(string email, string firstName, string resetLink)
    {
        const string subject = "Password Reset Request";
        var text = $@"Hello {firstName},

        We received a request to reset your password for your UA Backbone account.

        If you made this request, please reset your password using the link below:
        {resetLink}

        This link will expire in 1 hour. If you did not request a password reset, you can safely ignore this email.";
            var html = $@"<p>Hello {firstName},</p>
        <p>We received a request to reset your password for your <strong>UA Backbone</strong> account.</p>
        <p>If you made this request, click the link below to reset your password:</p>
        <p><a href=""{resetLink}"" style=""background-color:#b91c1c; color:#fff; padding:10px 16px; text-decoration:none; border-radius:6px;"">Reset Password</a></p>
        <p>This link will expire in 1 hour. If you did not request a password reset, you can safely ignore this email.</p>";

        return SendAsync(email, firstName, subject, text: text, html: html);
    }

    public Task<HttpResponseMessage> SendPendingAsync(string email, string firstName)
    {
        const string subject = "Registration Received";
        var text = $@"Hello {firstName},

        We’ve received your registration for UA Backbone and it’s now in the review queue.
        We’ll send you another email as soon as your account is approved.

        No action is needed from you right now.";
            var html = $@"<p>Hello {firstName},</p>
        <p>We’ve received your registration for <strong>UA Backbone</strong> and it’s now in the review queue.</p>
        <p>We’ll email you again as soon as your account is approved. No action is needed from you right now.</p>";

        return SendAsync(email, firstName, subject, text: text, html: html);
    }

    public Task<HttpResponseMessage> SendApprovedAsync(string email, string firstName)
    {
        const string subject = "Account Approved";
        var text = $@"Hi {firstName},

        Your account has been approved. You can now log in!";
        var html = $@"<p>Hi {firstName},</p>
        <p>Your account has been approved. You can now log in!</p>";

        return SendAsync(email, firstName, subject, text: text, html: html);
    }
}
