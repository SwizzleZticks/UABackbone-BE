using System.ComponentModel.DataAnnotations;

namespace UABackbone_Backend.DTOs;

public class RegisterDto
{
    [Required]    
    public required string Username { get; set; } = null!;
    [Required]
    public required string Password { get; set; } = null!;
    [Required]
    public required string FirstName { get; set; } = null!;
    [Required]
    public required string LastName { get; set; } = null!;
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public required string Email { get; set; } = null!;
    [Required]
    public short Local { get; set; }
}

