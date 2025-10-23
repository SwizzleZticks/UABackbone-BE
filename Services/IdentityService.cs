using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.Controllers;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Services
{
    public class IdentityService(RailwayContext context) : IIdentityService
    {
        public async Task<bool> UsernameExistsAsync(string? username)
        {
            return await context.Users.AnyAsync(u => u.Username == username) ||
                   await context.PendingUsers.AnyAsync(u => u.Username == username);
        }
        public async Task<bool> EmailExistsAsync(string? email)
        {
            return await context.Users.AnyAsync(u => u.Email == email) ||
                   await context.PendingUsers.AnyAsync(u => u.Email == email);
        }
    }
}
