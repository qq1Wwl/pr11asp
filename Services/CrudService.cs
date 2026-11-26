using AutoMapper;
using pr9ASP.Repositories;

namespace pr9ASP.Services
{
    /// <summary>
    /// Общая бизнес-логика CRUD: пагинация, проверки, маппинг через AutoMapper.
    /// Наследники добавляют проверки внешних ключей через Validate*-методы,
    /// а доработку сущности после маппинга (например, хеширование пароля) — через AfterMap*-методы.
    /// </summary>
    public abstract class CrudService<TEntity, TDto, TCreate, TUpdate, TPatch>
        : ICrudService<TDto, TCreate, TUpdate, TPatch>
        where TEntity : class
    {
        protected readonly IRepository<TEntity> Repository;
        protected readonly IMapper Mapper;

        protected CrudService(IRepository<TEntity> repository, IMapper mapper)
        {
            Repository = repository;
            Mapper = mapper;
        }

        // ---- Точки расширения: проверка существования связанных сущностей ----
        protected virtual Task ValidateCreateAsync(TCreate dto, CancellationToken ct) => Task.CompletedTask;
        protected virtual Task ValidateUpdateAsync(int id, TUpdate dto, CancellationToken ct) => Task.CompletedTask;
        protected virtual Task ValidatePatchAsync(int id, TPatch dto, CancellationToken ct) => Task.CompletedTask;

        // ---- Точки расширения: доработка сущности сразу после маппинга ----
        protected virtual void AfterMapCreate(TEntity entity, TCreate dto) { }
        protected virtual void AfterMapUpdate(TEntity entity, TUpdate dto) { }
        protected virtual void AfterMapPatch(TEntity entity, TPatch dto) { }

        protected static async Task EnsureExistsAsync<T>(
            IRepository<T> repository, int id, string what, CancellationToken ct) where T : class
        {
            if (!await repository.ExistsAsync(id, ct))
                throw new ReferenceNotFoundException($"{what} с Id {id} не найден(а).");
        }

        public async Task<(List<TDto> Items, int TotalCount)> GetAllAsync(
            int page, int pageSize, CancellationToken ct)
        {
            (page, pageSize) = Paging.Normalize(page, pageSize);

            var entities = await Repository.GetPageAsync(page, pageSize, ct);
            var total = await Repository.CountAsync(ct);
            return (Mapper.Map<List<TDto>>(entities), total);
        }

        public async Task<(List<TDto> Items, int TotalCount)> GetByOwnerAsync(
            int ownerId, int page, int pageSize, CancellationToken ct)
        {
            (page, pageSize) = Paging.Normalize(page, pageSize);

            var entities = await Repository.GetPageByOwnerAsync(ownerId, page, pageSize, ct);
            var total = await Repository.CountByOwnerAsync(ownerId, ct);
            return (Mapper.Map<List<TDto>>(entities), total);
        }

        public async Task<TDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            var entity = await Repository.GetByIdNoTrackingAsync(id, ct);
            return entity is null ? default : Mapper.Map<TDto>(entity);
        }

        public async Task<TDto> CreateAsync(TCreate dto, CancellationToken ct)
        {
            await ValidateCreateAsync(dto, ct);

            var entity = Mapper.Map<TEntity>(dto);
            AfterMapCreate(entity, dto);
            await Repository.AddAsync(entity, ct);
            await Repository.SaveChangesAsync(ct);

            return await ReadFreshAsync(Repository.GetKey(entity), ct);
        }

        public async Task<TDto?> UpdateAsync(int id, TUpdate dto, CancellationToken ct)
        {
            var entity = await Repository.GetByIdAsync(id, ct);
            if (entity is null) return default;

            await ValidateUpdateAsync(id, dto, ct);

            Mapper.Map(dto, entity);
            AfterMapUpdate(entity, dto);
            Repository.Update(entity);
            await Repository.SaveChangesAsync(ct);

            return await ReadFreshAsync(id, ct);
        }

        public async Task<TDto?> PatchAsync(int id, TPatch dto, CancellationToken ct)
        {
            var entity = await Repository.GetByIdAsync(id, ct);
            if (entity is null) return default;

            await ValidatePatchAsync(id, dto, ct);

            // AutoMapper копирует только непустые поля (см. MappingProfile)
            Mapper.Map(dto, entity);
            AfterMapPatch(entity, dto);
            Repository.Update(entity);
            await Repository.SaveChangesAsync(ct);

            return await ReadFreshAsync(id, ct);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var entity = await Repository.GetByIdAsync(id, ct);
            if (entity is null) return false;

            if (await Repository.HasDependentsAsync(id, ct))
                throw new ConflictException("Нельзя удалить: на эту запись ссылаются другие записи.");

            Repository.Remove(entity);
            await Repository.SaveChangesAsync(ct);
            return true;
        }

        // Перечитываем из БД (с Include), чтобы в ответе были актуальные вложенные данные
        private async Task<TDto> ReadFreshAsync(int id, CancellationToken ct)
        {
            var fresh = await Repository.GetByIdNoTrackingAsync(id, ct)
                ?? throw new InvalidOperationException("Не удалось перечитать сохранённую запись.");
            return Mapper.Map<TDto>(fresh);
        }
    }
}
