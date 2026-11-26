namespace pr9ASP.Services
{
    // Service ничего не знает про HTTP — ни ActionResult, ни статус-кодов.
    // Not-found выражается через null/false (HTTP-статус выбирает Controller),
    // нарушение правил — через ReferenceNotFoundException / ConflictException.
    public interface ICrudService<TDto, TCreate, TUpdate, TPatch>
    {
        Task<(List<TDto> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct);

        /// <summary>Записи конкретного владельца (для сущностей с User_Id): «мои заказы», «мои корзины».</summary>
        Task<(List<TDto> Items, int TotalCount)> GetByOwnerAsync(
            int ownerId, int page, int pageSize, CancellationToken ct);

        Task<TDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<TDto> CreateAsync(TCreate dto, CancellationToken ct);
        Task<TDto?> UpdateAsync(int id, TUpdate dto, CancellationToken ct);
        Task<TDto?> PatchAsync(int id, TPatch dto, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
    }
}
