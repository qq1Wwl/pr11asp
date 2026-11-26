using pr9ASP.Models;

namespace pr9ASP.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> LoginExistsAsync(string login, int? excludeId, CancellationToken ct);

        /// <summary>Пользователь по логину вместе с ролью (отслеживается — для пересохранения хеша).</summary>
        Task<User?> FindByLoginAsync(string login, CancellationToken ct);
    }
}
