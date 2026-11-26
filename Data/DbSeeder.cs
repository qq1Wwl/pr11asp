using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using pr9ASP.Auth;
using pr9ASP.Models;

namespace pr9ASP.Data
{
    /// <summary>
    /// При старте гарантирует наличие ролей Admin/Manager/User и первого администратора.
    /// Админа нельзя вставить обычным SQL — нужен хеш PasswordHasher, поэтому он создаётся здесь.
    /// Логин и пароль берутся из секции "Seed" в appsettings.json.
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                var adminRole = await EnsureRoleAsync(context, AppRoles.Admin);
                await EnsureRoleAsync(context, AppRoles.Manager);
                await EnsureRoleAsync(context, AppRoles.User);

                var login = config["Seed:AdminLogin"];
                var password = config["Seed:AdminPassword"];
                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    logger.LogWarning("Seed:AdminLogin / Seed:AdminPassword не заданы — администратор не создан.");
                    return;
                }

                if (!await context.Users.AnyAsync(u => u.Login == login))
                {
                    var admin = new User
                    {
                        Login = login,
                        Role_Id = adminRole.Role_Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    admin.PasswordHash = hasher.HashPassword(admin, password);

                    context.Users.Add(admin);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Создан администратор «{Login}».", login);
                }
            }
            catch (Exception ex)
            {
                // Приложение должно стартовать даже если скрипт add_auth.sql ещё не выполнен
                logger.LogError(ex,
                    "Не удалось выполнить начальное заполнение. Проверьте, что выполнен Scripts/add_auth.sql.");
            }
        }

        private static async Task<Role> EnsureRoleAsync(ShopDbContext context, string name)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == name);
            if (role is null)
            {
                role = new Role { RoleName = name, CreatedAt = DateTime.UtcNow };
                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }
            return role;
        }
    }
}
