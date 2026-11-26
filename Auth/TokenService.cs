using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using pr9ASP.Models;

namespace pr9ASP.Auth
{
    public interface ITokenService
    {
        /// <summary>Создаёт access-токен. У пользователя должна быть загружена Role.</summary>
        string CreateAccessToken(User user);
    }

    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public string CreateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                // минимум по заданию: Id и роль
                new(AppClaims.UserId, user.User_Id.ToString()),
                new(AppClaims.Role, user.Role.RoleName),
                new(AppClaims.Login, user.Login),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiresMinutes);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
