namespace pr9ASP.Models
{
    public class User
    {
        public int User_Id { get; set; }
        public string Login { get; set; } = string.Empty;

        /// <summary>Хеш пароля (PasswordHasher&lt;User&gt;). Пароль в открытом виде не хранится.</summary>
        public string PasswordHash { get; set; } = string.Empty;

        public int Role_Id { get; set; }
        public virtual Role Role { get; set; } = null!;

        /// <summary>Неактивный пользователь не может войти.</summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
