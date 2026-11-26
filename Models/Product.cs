namespace pr9ASP.Models
{
    public class Product
    {
        public int Product_Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        /// <summary>Сколько единиц товара сейчас свободно (не зарезервировано).</summary>
        public int Stock { get; set; }

        public int Category_Id { get; set; }
        public virtual Category Category { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
