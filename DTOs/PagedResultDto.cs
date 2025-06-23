using System.Collections.Generic;

namespace UABackbone_Backend.DTOs;

public class PagedResultDto<T>
{
    public int Total { get; set; }
    public List<T> Items { get; set; } = new List<T>();
}