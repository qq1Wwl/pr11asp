using pr9ASP.Models;

namespace pr9ASP.Repositories
{
    public interface IReservationRepository : IRepository<Reservation>
    {
        /// <param name="userId">Если задан — только резервы этого пользователя.</param>
        Task<List<Reservation>> GetFilteredPageAsync(
            int page, int pageSize, bool activeOnly, int? userId, CancellationToken ct);

        Task<int> CountFilteredAsync(bool activeOnly, int? userId, CancellationToken ct);

        /// <summary>Активный (не отменённый) резерв вместе с товаром; иначе null.</summary>
        Task<Reservation?> FindActiveAsync(int reservationId, CancellationToken ct);
    }
}
