namespace pr9ASP.Auth
{
    /// <summary>Названия ролей (значения Roles.RoleName в базе).</summary>
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string User = "User";
    }

    /// <summary>Имена claims в токене (короткие, как принято в JWT).</summary>
    public static class AppClaims
    {
        public const string UserId = "sub";
        public const string Role = "role";
        public const string Login = "name";
    }

    /// <summary>Названия политик авторизации.</summary>
    public static class Policies
    {
        /// <summary>Role-based: доступ только роли Admin.</summary>
        public const string AdminOnly = "AdminOnly";

        /// <summary>Role-based с двумя ролями: Admin и Manager (управление товарами и категориями).</summary>
        public const string CanManageProducts = "CanManageProducts";

        /// <summary>Resource-based: владелец ресурса или Admin.</summary>
        public const string OwnerOrAdmin = "OwnerOrAdmin";
    }
}
