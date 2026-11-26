namespace pr9ASP.Models
{
    public class Role
    {
        public int Role_Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
