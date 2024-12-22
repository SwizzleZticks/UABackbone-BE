using System;
using System.Collections.Generic;

namespace UABackbone_Backend.Models;

public partial class TestEf
{
    public ulong Id { get; set; }

    public string? Language { get; set; }

    public string? Food { get; set; }
}
