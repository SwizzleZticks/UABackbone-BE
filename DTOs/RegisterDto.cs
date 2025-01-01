using System.ComponentModel.DataAnnotations;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.DTOs;

public class RegisterDto
{
    [Required]    
    public required string Username { get; set; } = null!;
    [Required]
    public required string PasswordHash { get; set; } = null!;
    [Required]
    public required string FirstName { get; set; } = null!;
    [Required]
    public required string LastName { get; set; } = null!;
    [Required]
    public required string Email { get; set; } = null!;
    [Required]
    public short Local { get; set; }
}

