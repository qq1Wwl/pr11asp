using System.ComponentModel.DataAnnotations;

namespace pr9ASP.DTOs
{
    // Чтение — с вложенной категорией
    public class ProductDto
    {
        public int Product_Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int Category_Id { get; set; }
        public CategoryDto Category { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    // Краткая форма товара для вложения в другие DTO
    public class ProductShortDto
    {
        public int Product_Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    // Запись — с Id связанной категории
    public class CreateProductDto
    {
        [Required, StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Range(0, 1_000_000_000)]
        public decimal Price { get; set; }

        [Range(0, 1_000_000)]
        public int Stock { get; set; }

        public int Category_Id { get; set; }
    }

    public class UpdateProductDto
    {
        [Required, StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Range(0, 1_000_000_000)]
        public decimal Price { get; set; }

        [Range(0, 1_000_000)]
        public int Stock { get; set; }

        public int Category_Id { get; set; }
    }

    public class PatchProductDto
    {
        [StringLength(200)]
        public string? ProductName { get; set; }

        [Range(0, 1_000_000_000)]
        public decimal? Price { get; set; }

        [Range(0, 1_000_000)]
        public int? Stock { get; set; }

        public int? Category_Id { get; set; }
    }
}
