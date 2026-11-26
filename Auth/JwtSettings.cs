namespace pr9ASP.Auth
{
    /// <summary>Настройки JWT из секции "Jwt" в appsettings.json.</summary>
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;

        /// <summary>Секретный ключ подписи HS256. Минимум 32 символа.</summary>
        public string Key { get; set; } = string.Empty;

        public int ExpiresMinutes { get; set; } = 30;
    }
}
