using pr9ASP.DTOs;

namespace pr9ASP.Services
{
    public interface IReservationService
    {
        /// <param name="userId">Если задан — только резервы этого пользователя.</param>
        Task<(List<ReservationDto> Items, int TotalCount)> GetAllAsync(
            int page, int pageSize, bool activeOnly, int? userId, CancellationToken ct);

        Task<ReservationDto?> GetByIdAsync(int id, CancellationToken ct);

        /// <summary>
        /// Зарезервировать товар: Product.Stock уменьшается, создаётся запись Reservation.
        /// null — товар не найден; ReferenceNotFoundException — пользователь не найден;
        /// ConflictException — не хватает товара на складе.
        /// </summary>
        Task<ReservationDto?> ReserveAsync(ReserveProductDto dto, CancellationToken ct);

        /// <summary>
        /// Отменить резерв: Product.Stock возвращается, Reservation закрывается (ReleasedAt).
        /// null — активного резерва с таким Id нет.
        /// </summary>
        Task<ReservationDto?> CancelAsync(int reservationId, CancellationToken ct);
    }
}
