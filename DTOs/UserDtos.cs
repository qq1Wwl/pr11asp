using System.ComponentModel.DataAnnotations;

namespace pr9ASP.DTOs
{
    // Чтение — с вложенной ролью, без хеша пароля
    public class UserDto
    {
        public int User_Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public int Role_Id { get; set; }
        public RoleDto Role { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Краткая форма пользователя для вложения в другие DTO
    public class UserShortDto
    {
        public int User_Id { get; set; }
        public string Login { get; set; } = string.Empty;
    }

    // Запись (создаёт администратор). Приходит пароль — хеш считает сервис.
    public class CreateUserDto
    {
        [Required, StringLength(50, MinimumLength = 3)]
        public string Login { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        public int Role_Id { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateUserDto
    {
        [Required, StringLength(50, MinimumLength = 3)]
        public string Login { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        public int Role_Id { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class PatchUserDto
    {
        [StringLength(50, MinimumLength = 3)]
        public string? Login { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }

        public int? Role_Id { get; set; }
        public bool? IsActive { get; set; }
    }
}
