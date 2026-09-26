namespace UABackbone_Backend.DTOs
{
    public class UpdateUserDto
    {
        public string Username  { get; set; } = null!;
        public string Email     { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName  { get; set; } = null!;
        public int LocalId      { get; set; }
    }
}
