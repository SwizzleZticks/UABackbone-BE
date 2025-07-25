using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UABackbone_Backend.DTOs;

[NotMapped]
public class EmailContactDto
{
    [Required]
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    [Required]
    [JsonPropertyName("name")]
    public string Name {  get; set; }


}