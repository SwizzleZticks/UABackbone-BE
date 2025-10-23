using System.ComponentModel.DataAnnotations;

namespace UABackbone_Backend.DTOs;

public class ResetPasswordDto
{
    [Required]
    public string Token       { get; set; }
    [Required]
    [MinLength(8,  ErrorMessage = "Password must be at least 8 characters long.")]
    public string NewPassword { get; set; }
}