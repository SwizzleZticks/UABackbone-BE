using System;
using System.Collections.Generic;

namespace UABackbone_Backend.Models;

public partial class User
{
    public ushort Id { get; set; }

    public required string Username { get; set; } = null!;

    public required string PasswordHash { get; set; } = null!;

    public required string FirstName { get; set; } = null!;

    public required string LastName { get; set; } = null!;

    public required string Email { get; set; } = null!;

    public required short LocalId { get; set; }

    public required bool IsVerified { get; set; }

    public required bool IsAdmin { get; set; }

    public required bool IsBlacklisted { get; set; }

    public virtual LocalUnion Local { get; set; } = null!;
    
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpires { get; set; }
}
