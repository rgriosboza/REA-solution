namespace REA.API.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        Task<bool> ValidateTokenAsync(string token);
    }
}