using System.ComponentModel.DataAnnotations;

namespace UABackbone_Backend.DTOs
{
    public class AuthResponseDto
    {

        [Required]
        public required int Id { get; set; }
        [Required]
        public required string Username { get; set; } = null!;
        [Required]
        public required string FirstName { get; set; } = null!;
        [Required]
        public required string LastName { get; set; } = null!;
        [Required]
        public required string Email { get; set; } = null!;
        [Required]
        public required int Local { get; set; }
        [Required]
        public required bool IsAdmin { get; set; }
        [Required]
        public required bool IsBlacklisted { get; set; }
        public string Token { get; set; } = null!;
    }
}
