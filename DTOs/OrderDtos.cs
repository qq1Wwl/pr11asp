using System.ComponentModel.DataAnnotations;

namespace pr9ASP.DTOs
{
    // ---------- Order ----------

    // Чтение — с покупателем и позициями заказа
    public class OrderDto
    {
        public int Order_Id { get; set; }
        public int User_Id { get; set; }
        public UserShortDto User { get; set; } = null!;
        public List<OrderItemDto> Items { get; set; } = new();
        public DateTime OrderDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateOrderDto
    {
        public int User_Id { get; set; }
    }

    public class UpdateOrderDto
    {
        public int User_Id { get; set; }
    }

    public class PatchOrderDto
    {
        public int? User_Id { get; set; }
    }

    // ---------- OrderItem ----------

    // Чтение — с вложенным товаром
    public class OrderItemDto
    {
        public int OrderItem_Id { get; set; }
        public int Order_Id { get; set; }
        public int Product_Id { get; set; }
        public ProductShortDto Product { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateOrderItemDto
    {
        public int Order_Id { get; set; }
        public int Product_Id { get; set; }

        [Range(1, 100_000)]
        public int Quantity { get; set; }

        [Range(0, 1_000_000_000)]
        public decimal Price { get; set; }
    }

    public class UpdateOrderItemDto
    {
        public int Order_Id { get; set; }
        public int Product_Id { get; set; }

        [Range(1, 100_000)]
        public int Quantity { get; set; }

        [Range(0, 1_000_000_000)]
        public decimal Price { get; set; }
    }

    public class PatchOrderItemDto
    {
        public int? Order_Id { get; set; }
        public int? Product_Id { get; set; }

        [Range(1, 100_000)]
        public int? Quantity { get; set; }

        [Range(0, 1_000_000_000)]
        public decimal? Price { get; set; }
    }
}
