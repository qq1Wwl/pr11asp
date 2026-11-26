namespace pr9ASP.Models
{
    public class OrderItem
    {
        public int OrderItem_Id { get; set; }

        public int Order_Id { get; set; }
        public virtual Order Order { get; set; } = null!;

        public int Product_Id { get; set; }
        public virtual Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
