using System.ComponentModel.DataAnnotations;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.DTOs
{
    public class BlacklistedUserDto
    {
        public int      Id           { get; set; }
        [Required]
        public UserDto? UserAffected { get; set; }
        [Required]
        public UserDto? ByAdmin      { get; set; }
        [Required]
        [MinLength(1)]
        [MaxLength(500)]
        public string?  Reason       { get; set; }
        public DateTime Date         { get; set; }
    }
}
