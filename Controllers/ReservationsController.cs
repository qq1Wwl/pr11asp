using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pr9ASP.Auth;
using pr9ASP.DTOs;
using pr9ASP.Services;

namespace pr9ASP.Controllers
{
    [ApiController]
    [Route("api/reservations")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _service;
        private readonly IAuthorizationService _authorization;

        public ReservationsController(IReservationService service, IAuthorizationService authorization)
        {
            _service = service;
            _authorization = authorization;
        }

        // GET /api/reservations?page=1&pageSize=10&activeOnly=true — все резервы, только Admin
        [HttpGet]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
            [FromQuery] bool activeOnly = false, CancellationToken cancellationToken = default)
        {
            var (items, total) = await _service.GetAllAsync(
                page, pageSize, activeOnly, null, cancellationToken);

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(items);
        }

        // GET /api/reservations/my — резервы текущего пользователя
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMy(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
            [FromQuery] bool activeOnly = false, CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var (items, total) = await _service.GetAllAsync(
                page, pageSize, activeOnly, userId, cancellationToken);

            Response.Headers["X-Total-Count"] = total.ToString();
            return Ok(items);
        }

        // GET /api/reservations/5 — владелец резерва или Admin
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var item = await _service.GetByIdAsync(id, cancellationToken);
            if (item is null) return NotFound();

            var denied = await CheckAccessAsync(item);
            if (denied is not null) return denied;

            return Ok(item);
        }

        // POST /api/reservations — зарезервировать товар (для себя; Admin — для любого пользователя)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Reserve(
            [FromBody] ReserveProductDto dto, CancellationToken cancellationToken)
        {
            if (!User.IsAdmin())
            {
                var me = User.GetUserId();
                if (me is null) return Unauthorized();
                dto.User_Id = me.Value;
            }

            try
            {
                var item = await _service.ReserveAsync(dto, cancellationToken);
                if (item is null)
                    return NotFound(new { error = $"Товар с Id {dto.Product_Id} не найден." });

                return CreatedAtAction(nameof(GetById), new { id = item.Reservation_Id }, item);
            }
            catch (ConflictException ex)
            {
                // Не хватает товара на складе — понятная ошибка, а не 500
                return Conflict(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST /api/reservations/5/cancel — отменить резерв: владелец резерва или Admin
        [HttpPost("{id:int}/cancel")]
        [Authorize]
        public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
        {
            var existing = await _service.GetByIdAsync(id, cancellationToken);
            if (existing is not null)
            {
                var denied = await CheckAccessAsync(existing);
                if (denied is not null) return denied;
            }

            var item = await _service.CancelAsync(id, cancellationToken);
            if (item is null)
                return NotFound(new { error = $"Активный резерв с Id {id} не найден." });

            return Ok(item);
        }

        // Resource-based проверка: владелец резерва или Admin (обработчик OwnerOrAdminHandler)
        private async Task<IActionResult?> CheckAccessAsync(ReservationDto reservation)
        {
            var result = await _authorization.AuthorizeAsync(
                User, new OwnedResource(reservation.User_Id), Policies.OwnerOrAdmin);

            return result.Succeeded ? null : Forbid();
        }
    }
}
