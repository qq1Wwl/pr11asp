using System.ComponentModel.DataAnnotations;

namespace pr9ASP.DTOs
{
    // Чтение
    public class RoleDto
    {
        public int Role_Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Запись
    public class CreateRoleDto
    {
        [Required, StringLength(50)]
        public string RoleName { get; set; } = string.Empty;
    }

    public class UpdateRoleDto
    {
        [Required, StringLength(50)]
        public string RoleName { get; set; } = string.Empty;
    }

    public class PatchRoleDto
    {
        [StringLength(50)]
        public string? RoleName { get; set; }
    }
}
