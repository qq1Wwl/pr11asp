using Microsoft.EntityFrameworkCore;
using pr9ASP.Data;
using pr9ASP.Models;

namespace pr9ASP.Repositories
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        public ReservationRepository(ShopDbContext context) : base(context) { }

        protected override string KeyName => nameof(Reservation.Reservation_Id);
        protected override string? OwnerKeyName => nameof(Reservation.User_Id);

        protected override IQueryable<Reservation> Query() =>
            Context.Reservations.Include(r => r.User).Include(r => r.Product);

        public async Task<List<Reservation>> GetFilteredPageAsync(
            int page, int pageSize, bool activeOnly, int? userId, CancellationToken ct)
        {
            var query = Query().AsNoTracking();
            if (activeOnly) query = query.Where(r => r.ReleasedAt == null);
            if (userId is not null) query = query.Where(r => r.User_Id == userId);

            return await query
                .OrderByDescending(r => r.ReservedAt)
                .ThenBy(r => r.Reservation_Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<int> CountFilteredAsync(bool activeOnly, int? userId, CancellationToken ct)
        {
            var query = Context.Reservations.AsQueryable();
            if (activeOnly) query = query.Where(r => r.ReleasedAt == null);
            if (userId is not null) query = query.Where(r => r.User_Id == userId);
            return await query.CountAsync(ct);
        }

        public async Task<Reservation?> FindActiveAsync(int reservationId, CancellationToken ct) =>
            await Query().FirstOrDefaultAsync(
                r => r.Reservation_Id == reservationId && r.ReleasedAt == null, ct);
    }
}
