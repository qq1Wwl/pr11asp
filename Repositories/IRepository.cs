namespace pr9ASP.Repositories
{
    /// <summary>
    /// Универсальный репозиторий: только доступ к данным, без бизнес-логики.
    /// Все ключи в проекте — int.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetPageAsync(int page, int pageSize, CancellationToken ct);
        Task<int> CountAsync(CancellationToken ct);

        /// <summary>Страница записей, принадлежащих пользователю (для сущностей с владельцем).</summary>
        Task<List<T>> GetPageByOwnerAsync(int ownerId, int page, int pageSize, CancellationToken ct);
        Task<int> CountByOwnerAsync(int ownerId, CancellationToken ct);

        /// <summary>Сущность для изменения (отслеживается контекстом, со связанными данными).</summary>
        Task<T?> GetByIdAsync(int id, CancellationToken ct);

        /// <summary>Свежая копия из БД без отслеживания — для ответа клиенту.</summary>
        Task<T?> GetByIdNoTrackingAsync(int id, CancellationToken ct);

        Task<bool> ExistsAsync(int id, CancellationToken ct);

        /// <summary>Есть ли записи, которые ссылаются на эту сущность (мешают удалению).</summary>
        Task<bool> HasDependentsAsync(int id, CancellationToken ct);

        int GetKey(T entity);

        Task AddAsync(T entity, CancellationToken ct);
        void Update(T entity);
        void Remove(T entity);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
