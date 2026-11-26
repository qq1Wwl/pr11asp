using Microsoft.EntityFrameworkCore;
using pr9ASP.Data;

namespace pr9ASP.Repositories
{
    // Единственный слой, которому разрешено видеть ShopDbContext.
    public abstract class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ShopDbContext Context;

        protected Repository(ShopDbContext context)
        {
            Context = context;
        }

        /// <summary>Имя свойства-ключа (Product_Id, Order_Id, ...).</summary>
        protected abstract string KeyName { get; }

        /// <summary>Имя свойства с Id владельца (User_Id) — только для сущностей с владельцем.</summary>
        protected virtual string? OwnerKeyName => null;

        /// <summary>Базовый запрос; наследники добавляют Include для вложенных данных.</summary>
        protected virtual IQueryable<T> Query() => Context.Set<T>();

        public async Task<List<T>> GetPageAsync(int page, int pageSize, CancellationToken ct) =>
            await Query()
                .AsNoTracking()
                .OrderBy(e => EF.Property<int>(e, KeyName))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

        public async Task<int> CountAsync(CancellationToken ct) =>
            await Context.Set<T>().CountAsync(ct);

        public async Task<List<T>> GetPageByOwnerAsync(
            int ownerId, int page, int pageSize, CancellationToken ct)
        {
            var ownerKey = RequireOwnerKey();
            return await Query()
                .AsNoTracking()
                .Where(e => EF.Property<int>(e, ownerKey) == ownerId)
                .OrderBy(e => EF.Property<int>(e, KeyName))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<int> CountByOwnerAsync(int ownerId, CancellationToken ct)
        {
            var ownerKey = RequireOwnerKey();
            return await Context.Set<T>()
                .CountAsync(e => EF.Property<int>(e, ownerKey) == ownerId, ct);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken ct) =>
            await Query().FirstOrDefaultAsync(e => EF.Property<int>(e, KeyName) == id, ct);

        public async Task<T?> GetByIdNoTrackingAsync(int id, CancellationToken ct) =>
            await Query().AsNoTracking().FirstOrDefaultAsync(e => EF.Property<int>(e, KeyName) == id, ct);

        public async Task<bool> ExistsAsync(int id, CancellationToken ct) =>
            await Context.Set<T>().AnyAsync(e => EF.Property<int>(e, KeyName) == id, ct);

        public virtual Task<bool> HasDependentsAsync(int id, CancellationToken ct) =>
            Task.FromResult(false);

        public int GetKey(T entity) =>
            (int)Context.Entry(entity).Property(KeyName).CurrentValue!;

        public async Task AddAsync(T entity, CancellationToken ct) =>
            await Context.Set<T>().AddAsync(entity, ct);

        // Помечаем только саму сущность; изменения коллекций EF увидит сам.
        public void Update(T entity) =>
            Context.Entry(entity).State = EntityState.Modified;

        public void Remove(T entity) => Context.Set<T>().Remove(entity);

        public async Task SaveChangesAsync(CancellationToken ct) =>
            await Context.SaveChangesAsync(ct);

        private string RequireOwnerKey() =>
            OwnerKeyName ?? throw new NotSupportedException(
                $"У сущности {typeof(T).Name} нет владельца.");
    }
}
