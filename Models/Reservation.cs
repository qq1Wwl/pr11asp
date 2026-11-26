namespace pr9ASP.Models
{
    /// <summary>
    /// Факт резервирования товара: кто (User), что (Product, Quantity), когда (ReservedAt).
    /// ReleasedAt == null — резерв активен; заполнено — резерв отменён и товар возвращён на склад.
    /// </summary>
    public class Reservation
    {
        public int Reservation_Id { get; set; }

        public int User_Id { get; set; }
        public virtual User User { get; set; } = null!;

        public int Product_Id { get; set; }
        public virtual Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReleasedAt { get; set; }
    }
}
