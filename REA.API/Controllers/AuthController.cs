using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REA.API.Services;
using REA.Models.DTOs;
using System.Security.Claims;

namespace REA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponse 
                { 
                    Success = false, 
                    Message = "Datos de login inválidos" 
                });
            }

            var result = await _authService.LoginAsync(request);
            
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new LoginResponse 
                { 
                    Success = false, 
                    Message = "Refresh token requerido" 
                });
            }

            var result = await _authService.RefreshTokenAsync(request);
            
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Usuario no autorizado" });
                }

                var result = await _authService.ChangePasswordAsync(userId, request);

                if (!result)
                {
                    return BadRequest(new { message = "No se pudo cambiar la contraseña. Verifique su contraseña actual." });
                }

                return Ok(new { message = "Contraseña cambiada exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { message = "Error al cambiar la contraseña" });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                var user = await _authService.GetUserByIdAsync(userId);
                
                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLogin,
                    IsActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, new { message = "Error al obtener información del usuario" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public ActionResult Logout()
        {
            // In a production system, you might want to invalidate the refresh token here
            // For now, we'll just return success
            return Ok(new { message = "Logout exitoso" });
        }

        [HttpGet("validate")]
        [Authorize]
        public ActionResult ValidateToken()
        {
            return Ok(new { valid = true, message = "Token válido" });
        }
    }
}