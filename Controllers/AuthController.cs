using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UABackbone_Backend.DTOs;
using UABackbone_Backend.Interfaces;
using UABackbone_Backend.Models;

namespace UABackbone_Backend.Controllers
{
    public class AuthController(RailwayContext context, IEmailService emailService, ITokenService tokenService, IIdentityService identityService) : BaseApiController
    {
        [HttpPost("register-pending")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<PendingUserDto>> PendingCreationAsync([FromForm] RegisterDto dto, IFormFile uaCard)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Basic upload guards
            if (uaCard is null || uaCard.Length == 0)
            {
                return BadRequest("UA card image is required.");
            }

            const long maxBytes = 5_000_000; // 5 MB
            if (uaCard.Length > maxBytes)
            {
                return BadRequest("UA card image must be 5 MB or smaller.");
            }

            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowed.Contains(uaCard.ContentType))
            {
                return BadRequest("Only JPEG, PNG, or WEBP images are allowed.");
            }

            var normalizedUsername = dto.Username?.Trim().ToLowerInvariant();
            var normalizedEmail = dto.Email?.Trim().ToLowerInvariant();

            if (await identityService.UsernameExistsAsync(normalizedUsername))
            {
                return BadRequest("Username field is required");
            }

            if (await identityService.EmailExistsAsync(normalizedEmail))
            {
                return BadRequest("Email field is required");
            }

            byte[] imageBytes;
            using (var ms = new MemoryStream())
            {
                await uaCard.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            var pendingUser = new PendingUser
            {
                Username     = normalizedUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password?.Trim()),
                FirstName    = dto.FirstName?.Trim(),
                LastName     = dto.LastName?.Trim(),
                Email        = normalizedEmail,
                Local        = dto.Local,
                UaCardImage  = imageBytes,          // consider object storage later
                SubmittedAt  = DateTime.UtcNow
            };

            context.PendingUsers.Add(pendingUser);
            await context.SaveChangesAsync();

            var mailResp = await emailService.SendPendingAsync(pendingUser.Email, pendingUser.FirstName ?? "");
            Console.WriteLine($"PENDING EMAIL STATUS {(int)mailResp.StatusCode}");

            var pendingUserDto = new PendingUserDto
            {
                Id          = pendingUser.Id,
                Username    = pendingUser.Username,
                FirstName   = pendingUser.FirstName,
                LastName    = pendingUser.LastName,
                Email       = pendingUser.Email,
                Local       = pendingUser.Local,
                SubmittedAt = DateTime.UtcNow
            };

            return Created("api/Account/register-pending", pendingUserDto);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> LoginAsync([FromBody] LoginDto loginDto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

            if (user == null)
            {
                return Unauthorized("Email does not exist");
            }

            if (user.IsBlacklisted)
            {
                return Unauthorized("This account has been blacklisted.");
            }

            var password = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!password)
            {
                return Unauthorized("Invalid password");
            }

            return new AuthResponseDto
            {
                Id        = user.Id,
                Username  = user.Username,
                FirstName = user.FirstName,
                LastName  = user.LastName,
                Email     = user.Email,
                Local     = user.LocalId,
                Token     = tokenService.CreateToken(user)
            };
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordDto dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (user == null)
            {
                return BadRequest("Email does not exist");
            }

            var token = tokenService.CreateToken(user, 1, "reset");

            user.PasswordResetToken = token;
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await context.SaveChangesAsync();

            var resetLink = $"http://localhost:4200/reset-password?token={token}";

            await emailService.SendResetLinkAsync(user.Email, user.FirstName, resetLink);

            return Ok(new { message = "Password reset link sent." });
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token);

            if (user == null)
            {
                return BadRequest("No user found");
            }

            if (user.PasswordResetTokenExpires < DateTime.UtcNow)
            {
                return Unauthorized("Password reset expired");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;
            await context.SaveChangesAsync();

            return Ok(new { message = "Password has been successfully reset." });
        }
    }
}
