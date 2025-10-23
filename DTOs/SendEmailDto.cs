using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UABackbone_Backend.DTOs;

[NotMapped]
public class SendEmailDto
{
    [Required]
    [JsonPropertyName("from")]
    public EmailContactDto        From    { get; set; }
    [Required]
    [JsonPropertyName("to")]
    public List<EmailContactDto>  To      { get; set; } = [];
    [Required]
    [JsonPropertyName("subject")]
    public string                 Subject { get; set; }
    [Required]
    [JsonPropertyName("text")]
    public string                 Text    { get; set; }
    [Required]
    [JsonPropertyName("html")]
    public string                 Html    { get; set; }
}