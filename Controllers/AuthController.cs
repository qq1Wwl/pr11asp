using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using pr9ASP.Auth;
using pr9ASP.DTOs;
using pr9ASP.Services;

namespace pr9ASP.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        /// <summary>Имя rate limiting-политики входа (см. Program.cs).</summary>
        public const string LoginRateLimitPolicy = "LoginPolicy";

        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwt;

        public AuthController(
            IUserService userService, ITokenService tokenService, IOptions<JwtSettings> jwt)
        {
            _userService = userService;
            _tokenService = tokenService;
            _jwt = jwt.Value;
        }

        // POST /api/auth/register — 201 + токен / 400 (валидация) / 409 (логин занят)
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] RegisterDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userService.RegisterAsync(dto.Login, dto.Password, cancellationToken);
                var accessToken = _tokenService.CreateAccessToken(user);

                return StatusCode(StatusCodes.Status201Created, new LoginResponseDto
                {
                    AccessToken = accessToken,
                    ExpiresInMinutes = _jwt.ExpiresMinutes,
                });
            }
            catch (ConflictException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST /api/auth/login
        // Ограничен rate limiting-политикой "LoginPolicy" (см. Program.cs) — не больше
        // 5 попыток в минуту с одного IP. Коды: 200 + токен / 401 / 429.
        [EnableRateLimiting(LoginRateLimitPolicy)]
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto dto, CancellationToken cancellationToken)
        {
            var user = await _userService.ValidateCredentialsAsync(
                dto.Login, dto.Password, cancellationToken);
            if (user is null)
                return Unauthorized(new { error = "Неверный логин или пароль." });

            var accessToken = _tokenService.CreateAccessToken(user);

            return Ok(new LoginResponseDto
            {
                AccessToken = accessToken,
                ExpiresInMinutes = _jwt.ExpiresMinutes,
            });
        }
    }
}
