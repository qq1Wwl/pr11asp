using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pr9ASP.Auth;
using pr9ASP.Services;

namespace pr9ASP.Controllers
{
    /// <summary>
    /// Тонкий базовый контроллер: вызывает сервис и переводит результат в HTTP-статус.
    /// Здесь только *Core-методы без маршрутов: конкретный контроллер сам объявляет действия
    /// и вешает на каждое нужные [HttpXxx] и [Authorize], а тело делегирует сюда.
    /// Так для каждой операции можно задать свою политику доступа.
    /// </summary>
    [ApiController]
    public abstract class CrudController<TDto, TCreate, TUpdate, TPatch> : ControllerBase
    {
        private readonly ICrudService<TDto, TCreate, TUpdate, TPatch> _service;
        private readonly Func<TDto, int> _getId;

        protected CrudController(
            ICrudService<TDto, TCreate, TUpdate, TPatch> service, Func<TDto, int> getId)
        {
            _service = service;
            _getId = getId;
        }

        // ---- Точки расширения для контроллеров с владельцем ресурса ----

        /// <summary>Нужно ли проверять доступ к конкретной записи (resource-based).</summary>
        protected virtual bool NeedsAccessCheck => false;

        /// <summary>Проверка доступа к записи. null — доступ есть, иначе готовый ответ (403).</summary>
        protected virtual Task<IActionResult?> CheckAccessAsync(TDto item) =>
            Task.FromResult<IActionResult?>(null);

        /// <summary>Подготовка/проверка входных данных. null — всё в порядке, иначе готовый ответ.</summary>
        protected virtual IActionResult? PrepareCreate(TCreate dto) => null;
        protected virtual IActionResult? PrepareUpdate(TUpdate dto) => null;
        protected virtual IActionResult? PreparePatch(TPatch dto) => null;

        // ---- Операции ----

        protected async Task<IActionResult> GetAllCore(int page, int pageSize, CancellationToken ct)
        {
            var (items, total) = await _service.GetAllAsync(page, pageSize, ct);

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(items);
        }

        protected async Task<IActionResult> GetMyCore(int page, int pageSize, CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var (items, total) = await _service.GetByOwnerAsync(userId.Value, page, pageSize, ct);

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(items);
        }

        protected async Task<IActionResult> GetByIdCore(int id, CancellationToken ct)
        {
            var item = await _service.GetByIdAsync(id, ct);
            if (item is null) return NotFound();

            if (NeedsAccessCheck)
            {
                var denied = await CheckAccessAsync(item);
                if (denied is not null) return denied;
            }

            return Ok(item);
        }

        protected async Task<IActionResult> CreateCore(TCreate dto, CancellationToken ct)
        {
            var invalid = PrepareCreate(dto);
            if (invalid is not null) return invalid;

            try
            {
                var item = await _service.CreateAsync(dto, ct);
                return CreatedAtAction("GetById", new { id = _getId(item) }, item);
            }
            catch (InvalidOperationException ex)
            {
                return ErrorResult(ex);
            }
        }

        protected async Task<IActionResult> UpdateCore(int id, TUpdate dto, CancellationToken ct)
        {
            var denied = await CheckExistingAsync(id, ct);
            if (denied is not null) return denied;

            var invalid = PrepareUpdate(dto);
            if (invalid is not null) return invalid;

            try
            {
                var item = await _service.UpdateAsync(id, dto, ct);
                if (item is null) return NotFound();
                return Ok(item);
            }
            catch (InvalidOperationException ex)
            {
                return ErrorResult(ex);
            }
        }

        protected async Task<IActionResult> PatchCore(int id, TPatch dto, CancellationToken ct)
        {
            var denied = await CheckExistingAsync(id, ct);
            if (denied is not null) return denied;

            var invalid = PreparePatch(dto);
            if (invalid is not null) return invalid;

            try
            {
                var item = await _service.PatchAsync(id, dto, ct);
                if (item is null) return NotFound();
                return Ok(item);
            }
            catch (InvalidOperationException ex)
            {
                return ErrorResult(ex);
            }
        }

        protected async Task<IActionResult> DeleteCore(int id, CancellationToken ct)
        {
            var denied = await CheckExistingAsync(id, ct);
            if (denied is not null) return denied;

            try
            {
                var deleted = await _service.DeleteAsync(id, ct);
                if (!deleted) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return ErrorResult(ex);
            }
        }

        // Для изменения/удаления сначала находим запись и проверяем, что она доступна пользователю.
        // Если записи нет — отдаём 404 (дальше сервис всё равно вернёт null/false).
        private async Task<IActionResult?> CheckExistingAsync(int id, CancellationToken ct)
        {
            if (!NeedsAccessCheck) return null;

            var item = await _service.GetByIdAsync(id, ct);
            if (item is null) return NotFound();

            return await CheckAccessAsync(item);
        }

        // ConflictException -> 409, остальные бизнес-ошибки (нет связанной записи) -> 400
        protected IActionResult ErrorResult(InvalidOperationException ex) =>
            ex is ConflictException
                ? Conflict(new { error = ex.Message })
                : BadRequest(new { error = ex.Message });
    }

    /// <summary>
    /// Контроллер сущности с владельцем (User_Id). Доступ к конкретной записи проверяет
    /// собственная resource-based политика OwnerOrAdmin: владелец — свои записи, Admin — любые.
    /// </summary>
    public abstract class OwnedCrudController<TDto, TCreate, TUpdate, TPatch>
        : CrudController<TDto, TCreate, TUpdate, TPatch>
    {
        private readonly IAuthorizationService _authorization;
        private readonly Func<TDto, int> _getOwnerId;

        protected OwnedCrudController(
            ICrudService<TDto, TCreate, TUpdate, TPatch> service,
            Func<TDto, int> getId,
            Func<TDto, int> getOwnerId,
            IAuthorizationService authorization) : base(service, getId)
        {
            _authorization = authorization;
            _getOwnerId = getOwnerId;
        }

        protected override bool NeedsAccessCheck => true;

        protected override async Task<IActionResult?> CheckAccessAsync(TDto item)
        {
            // Ресурс передаётся в обработчик OwnerOrAdminHandler
            var result = await _authorization.AuthorizeAsync(
                User, new OwnedResource(_getOwnerId(item)), Policies.OwnerOrAdmin);

            return result.Succeeded ? null : Forbid();
        }
    }
}
