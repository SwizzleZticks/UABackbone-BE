using System.ComponentModel.DataAnnotations;

namespace UABackbone_Backend.DTOs;

public class LoginDto
{
    [Required]
    public required string UserName { get; set; }
    [Required]
    public required string Password { get; set; }
}