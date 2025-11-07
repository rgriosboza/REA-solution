using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using REA.Models.Data;
using REA.Models.DTOs;
using REA.Models.Entities;

namespace REA.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context, 
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                // Find user by email or username
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => 
                        (u.Email == request.UsernameOrEmail || u.Username == request.UsernameOrEmail) 
                        && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent user: {User}", request.UsernameOrEmail);
                    return new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Usuario o contraseña incorrectos" 
                    };
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for user: {Username}", user.Username);
                    return new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Usuario o contraseña incorrectos" 
                    };
                }

                // Update last login
                user.LastLogin = DateTime.UtcNow;

                // Generate tokens
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                // Save refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
                    Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"] ?? "7"));

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successful login for user: {Username}", user.Username);

                return new LoginResponse
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    Message = "Login exitoso",
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Username = user.Username,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt,
                        LastLogin = user.LastLogin,
                        IsActive = user.IsActive
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {User}", request.UsernameOrEmail);
                return new LoginResponse 
                { 
                    Success = false, 
                    Message = "Error al procesar el login" 
                };
            }
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken && u.IsActive);

                if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Invalid or expired refresh token");
                    return new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Token de actualización inválido o expirado" 
                    };
                }

                // Generate new tokens
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                // Update refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
                    Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"] ?? "7"));

                await _context.SaveChangesAsync();

                return new LoginResponse
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Username = user.Username,
                        Role = user.Role
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return new LoginResponse 
                { 
                    Success = false, 
                    Message = "Error al actualizar token" 
                };
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                
                if (user == null || !user.IsActive)
                {
                    return false;
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogWarning("Failed password change - incorrect current password for user: {Username}", user.Username);
                    return false;
                }

                // Validate new password strength
                if (!IsPasswordStrong(request.NewPassword))
                {
                    throw new ArgumentException("La contraseña debe tener al menos 6 caracteres");
                }

                // Hash and save new password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Password changed successfully for user: {Username}", user.Username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var user = await _context.Users.FindAsync(userId);
                return user != null && user.IsActive;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Where(u => u.Id == userId && u.IsActive)
                .FirstOrDefaultAsync();
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("username", user.Username)
                }),
                Expires = DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"] ?? "60")),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private bool IsPasswordStrong(string password)
        {
            // At least 6 characters for now (can be made stricter)
            return password.Length >= 6;
        }
    }
}