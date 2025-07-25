using System.Net.Http.Headers;
using System.Text.Json;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.DTOs;

namespace UABackbone_Backend.Services;

public class EmailService(HttpClient client) : IEmailService
{
    public async Task<HttpResponseMessage> SendResetLinkAsync(string email, string name, string resetLink)
    {
        var emailDto = new SendEmailDto
        {
            From = new EmailContactDto
            {
                Email = "noreply@uabackbone.com",
                Name = "UA Backbone"
            },
            To =
            [
                new EmailContactDto
                {
                    Email = email,
                    Name = name
                }
            ],
            Subject = "Password Reset Link",
            Text = $"Click this link to reset your password: { resetLink }",
            Html = $"<p>Click <a href=\"{ resetLink }\">here</a> to reset your password.</p>"
        };
        
        string emailJson = JsonSerializer.Serialize(emailDto);
        StringContent httpContent = new StringContent(emailJson, System.Text.Encoding.UTF8, "application/json");
        
        return await client.PostAsync("email", httpContent);
    }
}