using FinanceApi.DTOs.Auth;
using FinanceApi.Services;
using FinanceApi.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            var (success, message, token, user) = await _authService.RegisterAsync(request.Email, request.Password, request.DisplayName);
            if (!success)
            {
                return Conflict(new { message });
            }

            return CreatedAtAction(nameof(Login), new { email = request.Email }, new AuthResponse
            {
                Token = token!,
                User = MapToDto(user!)
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var (success, message, token, user) = await _authService.LoginAsync(request.Email, request.Password);
            if (!success)
            {
                return Unauthorized(new { message });
            }

            return Ok(new AuthResponse
            {
                Token = token!,
                User = MapToDto(user!)
            });
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                DefaultCurrency = user.DefaultCurrency
            };
        }
    }
}
