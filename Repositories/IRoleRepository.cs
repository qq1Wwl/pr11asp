using pr9ASP.Models;

namespace pr9ASP.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> FindByNameAsync(string roleName, CancellationToken ct);
    }
}
