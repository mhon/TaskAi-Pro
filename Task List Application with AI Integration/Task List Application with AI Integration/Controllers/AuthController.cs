using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskAi.Api.Data;
using TaskAi.Api.Models;
using TaskAi.Api.Models.Auth;

namespace TaskAi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthController(AppDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email already registered.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully. Please delete your token on the client side." });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.Username ?? "")
            };

            // TEST: Hardcoding the key here to match the hardcoded key in Program.cs
            //var testKey = "abcdefghijklmnopqrstuvwxyzabcdef";
            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(testKey));

            var testKey = "abcdefghijklmnopqrstuvwxyzabcdef";
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(testKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60), // Hardcoded for test
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}