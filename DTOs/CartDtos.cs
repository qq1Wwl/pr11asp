using System.ComponentModel.DataAnnotations;

namespace pr9ASP.DTOs
{
    // ---------- Cart ----------

    // Чтение — с владельцем и позициями корзины
    public class CartDto
    {
        public int Cart_Id { get; set; }
        public int User_Id { get; set; }
        public UserShortDto User { get; set; } = null!;
        public List<CartItemDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCartDto
    {
        public int User_Id { get; set; }
    }

    public class UpdateCartDto
    {
        public int User_Id { get; set; }
    }

    public class PatchCartDto
    {
        public int? User_Id { get; set; }
    }

    // ---------- CartItem ----------

    // Чтение — с вложенным товаром
    public class CartItemDto
    {
        public int CartItem_Id { get; set; }
        public int Cart_Id { get; set; }
        public int Product_Id { get; set; }
        public ProductShortDto Product { get; set; } = null!;
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCartItemDto
    {
        public int Cart_Id { get; set; }
        public int Product_Id { get; set; }

        [Range(1, 100_000)]
        public int Quantity { get; set; }
    }

    public class UpdateCartItemDto
    {
        public int Cart_Id { get; set; }
        public int Product_Id { get; set; }

        [Range(1, 100_000)]
        public int Quantity { get; set; }
    }

    public class PatchCartItemDto
    {
        public int? Cart_Id { get; set; }
        public int? Product_Id { get; set; }

        [Range(1, 100_000)]
        public int? Quantity { get; set; }
    }
}
