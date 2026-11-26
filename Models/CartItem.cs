namespace pr9ASP.Models
{
    public class CartItem
    {
        public int CartItem_Id { get; set; }

        public int Cart_Id { get; set; }
        public virtual Cart Cart { get; set; } = null!;

        public int Product_Id { get; set; }
        public virtual Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
