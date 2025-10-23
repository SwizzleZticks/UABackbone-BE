namespace UABackbone_Backend.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> UsernameExistsAsync(string? username);
        Task<bool> EmailExistsAsync(string? email);
    }
}
