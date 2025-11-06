using REA.Models.DTOs;
using REA.Models.Entities;

namespace REA.API.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<bool> ValidateTokenAsync(string token);
        Task<User?> GetUserByIdAsync(int userId);
    }
}