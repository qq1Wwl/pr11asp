using System.Security.Claims;

namespace pr9ASP.Auth
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>Id пользователя из claim "sub"; null, если токена нет или claim некорректен.</summary>
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(AppClaims.UserId);
            return int.TryParse(value, out var id) ? id : null;
        }

        public static bool IsAdmin(this ClaimsPrincipal user) => user.IsInRole(AppRoles.Admin);
    }
}
