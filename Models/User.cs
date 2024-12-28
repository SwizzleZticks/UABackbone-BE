using System;
using System.Collections.Generic;

namespace UABackbone_Backend.Models;

public partial class User
{
    public ushort Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public short? LocalId { get; set; }

    public bool IsVerified { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsBlacklisted { get; set; }

    public virtual LocalUnion? Local { get; set; }
    
}
