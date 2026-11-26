using AutoMapper;
using pr9ASP.DTOs;
using pr9ASP.Models;
using pr9ASP.Repositories;

namespace pr9ASP.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservations;
        private readonly IRepository<Product> _products;
        private readonly IUserRepository _users;
        private readonly IMapper _mapper;

        public ReservationService(
            IReservationRepository reservations, IRepository<Product> products,
            IUserRepository users, IMapper mapper)
        {
            _reservations = reservations;
            _products = products;
            _users = users;
            _mapper = mapper;
        }

        public async Task<(List<ReservationDto> Items, int TotalCount)> GetAllAsync(
            int page, int pageSize, bool activeOnly, int? userId, CancellationToken ct)
        {
            (page, pageSize) = Paging.Normalize(page, pageSize);

            var items = await _reservations.GetFilteredPageAsync(page, pageSize, activeOnly, userId, ct);
            var total = await _reservations.CountFilteredAsync(activeOnly, userId, ct);
            return (_mapper.Map<List<ReservationDto>>(items), total);
        }

        public async Task<ReservationDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            var reservation = await _reservations.GetByIdNoTrackingAsync(id, ct);
            return reservation is null ? null : _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<ReservationDto?> ReserveAsync(ReserveProductDto dto, CancellationToken ct)
        {
            var product = await _products.GetByIdAsync(dto.Product_Id, ct);
            if (product is null) return null;

            if (!await _users.ExistsAsync(dto.User_Id, ct))
                throw new ReferenceNotFoundException($"Пользователь с Id {dto.User_Id} не найден.");

            if (product.Stock < dto.Quantity)
                throw new ConflictException(
                    $"Недостаточно товара «{product.ProductName}»: на складе {product.Stock}, запрошено {dto.Quantity}.");

            // Меняем две связанные сущности...
            product.Stock -= dto.Quantity;                      // 1) ресурс уменьшается

            var reservation = _mapper.Map<Reservation>(dto);    // 2) создаётся запись о событии
            reservation.Product = product;
            await _reservations.AddAsync(reservation, ct);

            // ...и сохраняем их ОДНИМ вызовом: репозитории работают с одним Scoped-DbContext,
            // поэтому SaveChangesAsync пишет и Product, и Reservation сразу. EF Core сам
            // выполняет один SaveChanges в одной транзакции — явная транзакция не нужна.
            await _reservations.SaveChangesAsync(ct);

            var fresh = await _reservations.GetByIdNoTrackingAsync(_reservations.GetKey(reservation), ct);
            return _mapper.Map<ReservationDto>(fresh);
        }

        public async Task<ReservationDto?> CancelAsync(int reservationId, CancellationToken ct)
        {
            var reservation = await _reservations.FindActiveAsync(reservationId, ct);
            if (reservation is null) return null;

            reservation.ReleasedAt = DateTime.UtcNow;           // 1) запись закрывается
            reservation.Product.Stock += reservation.Quantity;  // 2) ресурс возвращается

            await _reservations.SaveChangesAsync(ct);           // один вызов на обе сущности

            var fresh = await _reservations.GetByIdNoTrackingAsync(reservationId, ct);
            return _mapper.Map<ReservationDto>(fresh);
        }
    }
}
