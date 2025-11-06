using Microsoft.AspNetCore.Mvc;
using REA.API.Services;
using REA.Models.DTOs;

namespace REA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            
            if (result.Success)
                return Ok(result);
            else
                return Unauthorized(result);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            
            if (result.Success)
                return Ok(result);
            else
                return Unauthorized(result);
        }
    }
}