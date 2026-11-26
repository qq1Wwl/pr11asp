using System.ComponentModel.DataAnnotations;

namespace pr9ASP.DTOs
{
    public class RegisterDto
    {
        [Required, StringLength(50, MinimumLength = 3)]
        public string Login { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>Ответ на регистрацию и вход (одинаковый для Register и Login).</summary>
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; }
    }
}
