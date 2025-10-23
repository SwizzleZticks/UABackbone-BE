using System.ComponentModel.DataAnnotations;

namespace UABackbone_Backend.DTOs;

public class UserDto
{
    [Required] 
    public required int    Id        { get; set; }
    [Required]    
    public required string Username  { get; set; } = null!;
    [Required]
    public required string FirstName { get; set; } = null!;
    [Required]
    public required string LastName  { get; set; } = null!;
    [Required]
    public required string Email     { get; set; } = null!;
    [Required]
    public required int    Local     { get; set; }
    public string          Token     { get; set; } = null!;
}