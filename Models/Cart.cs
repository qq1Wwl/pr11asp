namespace pr9ASP.Models
{
    public class Cart
    {
        public int Cart_Id { get; set; }

        public int User_Id { get; set; }
        public virtual User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
