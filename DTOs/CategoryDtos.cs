using System.ComponentModel.DataAnnotations;

namespace pr9ASP.DTOs
{
    // Чтение
    public class CategoryDto
    {
        public int Category_Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Запись
    public class CreateCategoryDto
    {
        [Required, StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;
    }

    public class UpdateCategoryDto
    {
        [Required, StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;
    }

    public class PatchCategoryDto
    {
        [StringLength(100)]
        public string? CategoryName { get; set; }
    }
}
