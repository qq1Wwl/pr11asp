using System.ComponentModel.DataAnnotations;

namespace pr9ASP.DTOs
{
    // Чтение — кто, что, когда
    public class ReservationDto
    {
        public int Reservation_Id { get; set; }
        public int User_Id { get; set; }
        public UserShortDto User { get; set; } = null!;
        public int Product_Id { get; set; }
        public ProductShortDto Product { get; set; } = null!;
        public int Quantity { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public bool IsActive { get; set; }
    }

    // Запись — зарезервировать товар
    public class ReserveProductDto
    {
        public int User_Id { get; set; }
        public int Product_Id { get; set; }

        [Range(1, 100_000)]
        public int Quantity { get; set; }
    }
}
