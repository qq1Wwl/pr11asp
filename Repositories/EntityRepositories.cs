using Microsoft.EntityFrameworkCore;
using pr9ASP.Data;
using pr9ASP.Models;

namespace pr9ASP.Repositories
{
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        public RoleRepository(ShopDbContext context) : base(context) { }

        protected override string KeyName => nameof(Role.Role_Id);

        public async Task<Role?> FindByNameAsync(string roleName, CancellationToken ct) =>
            await Context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName, ct);

        public override async Task<bool> HasDependentsAsync(int id, CancellationToken ct) =>
            await Context.Users.AnyAsync(u => u.Role_Id == id, ct);
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ShopDbContext context) : base(context) { }

        protected override string KeyName => nameof(User.User_Id);

        protected override IQueryable<User> Query() =>
            Context.Users.Include(u => u.Role);

        public async Task<bool> LoginExistsAsync(string login, int? excludeId, CancellationToken ct) =>
            await Context.Users.AnyAsync(
                u => u.Login == login && (excludeId == null || u.User_Id != excludeId), ct);

        public async Task<User?> FindByLoginAsync(string login, CancellationToken ct) =>
            await Context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Login == login, ct);

        public override async Task<bool> HasDependentsAsync(int id, CancellationToken ct) =>
            await Context.Carts.AnyAsync(c => c.User_Id == id, ct)
            || await Context.Orders.AnyAsync(o => o.User_Id == id, ct)
            || await Context.Reservations.AnyAsync(r => r.User_Id == id, ct);
    }

    public class CategoryRepository : Repository<Category>
    {
        public CategoryRepository(ShopDbContext context) : base(context) { }

        protected override string KeyName => nameof(Category.Category_Id);

        public override async Task<bool> HasDependentsAsync(int id, CancellationToken ct) =>
            await Context.Products.AnyAsync(p => p.Category_Id == id, ct);
    }

    public class ProductRepository : Repository<Product>
    {
        public ProductRepository(ShopDbContext context) : base(context) { }

        protected override string KeyName => nameof(Product.Product_Id);

        protected override IQueryable<Product> Query() =>
            Context.Products.Include(p => p.Category);

        public override async Task<bool> HasDependentsAsync(int id, CancellationToken ct) =>
            await Context.CartItems.AnyAsync(i => i.Product_Id == id, ct)
            || await Context.OrderItems.AnyAsync(i => i.Product_Id == id, ct)
            || await Context.Reservations.AnyAsync(r => r.Product_Id == id, ct);
    }

    // Позиции корзины подгружаются, чтобы EF удалил их вместе с корзиной (Cascade)
    public class CartRepository : Repository<Cart>
    {
        public CartRepository(ShopDbContext context) : base(context) { }

        protected override string KeyName => nameof(Cart.Cart_Id);
        protected override string? OwnerKeyName => nameof(Cart.User_Id);

        protected override IQueryable<Cart> Query() =>
            Context.Carts
                .Include(c => c.User)
                .Include(c => c.Items).ThenInclude(i => i.Product);
    }

    public class CartItemRepository : Repository<CartItem>
    {
        public CartItemRepository(ShopDbContext context) : base(context) { }

        protected override string KeyName => nameof(CartItem.CartItem_Id);

        protected override IQueryable<CartItem> Query() =>
            Context.CartItems.Include(i => i.Product);
    }

    // Позиции заказа подгружаются, чтобы EF удалил их вместе с заказом (Cascade)
    public class OrderRepository : Repository<Order>
    {
        public OrderRepository(ShopDbContext context) : base(context) { }

        protected override string KeyName => nameof(Order.Order_Id);
        protected override string? OwnerKeyName => nameof(Order.User_Id);

        protected override IQueryable<Order> Query() =>
            Context.Orders
                .Include(o => o.User)
                .Include(o => o.Items).ThenInclude(i => i.Product);
    }

    public class OrderItemRepository : Repository<OrderItem>
    {
        public OrderItemRepository(ShopDbContext context) : base(context) { }

        protected override string KeyName => nameof(OrderItem.OrderItem_Id);

        protected override IQueryable<OrderItem> Query() =>
            Context.OrderItems.Include(i => i.Product);
    }
}
