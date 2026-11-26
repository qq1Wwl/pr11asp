namespace pr9ASP.Models
{
    public class Order
    {
        public int Order_Id { get; set; }

        public int User_Id { get; set; }
        public virtual User User { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
