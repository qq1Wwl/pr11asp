using AutoMapper;
using Microsoft.AspNetCore.Identity;
using pr9ASP.Auth;
using pr9ASP.DTOs;
using pr9ASP.Models;
using pr9ASP.Repositories;

namespace pr9ASP.Services
{
    // ---------- Интерфейсы ----------
    public interface IRoleService : ICrudService<RoleDto, CreateRoleDto, UpdateRoleDto, PatchRoleDto> { }
    public interface IUserService : ICrudService<UserDto, CreateUserDto, UpdateUserDto, PatchUserDto>
    {
        /// <summary>
        /// Регистрация обычного пользователя (роль User): логин должен быть свободен,
        /// пароль сохраняется только в виде хеша. ConflictException — логин занят.
        /// </summary>
        Task<User> RegisterAsync(string login, string password, CancellationToken ct = default);

        /// <summary>
        /// Проверка логина и пароля при входе. null — пользователь не найден, неактивен
        /// или пароль неверный (причину не раскрываем).
        /// </summary>
        Task<User?> ValidateCredentialsAsync(string login, string password, CancellationToken ct = default);
    }
    public interface ICategoryService : ICrudService<CategoryDto, CreateCategoryDto, UpdateCategoryDto, PatchCategoryDto> { }
    public interface IProductService : ICrudService<ProductDto, CreateProductDto, UpdateProductDto, PatchProductDto> { }
    public interface ICartService : ICrudService<CartDto, CreateCartDto, UpdateCartDto, PatchCartDto> { }
    public interface ICartItemService : ICrudService<CartItemDto, CreateCartItemDto, UpdateCartItemDto, PatchCartItemDto> { }
    public interface IOrderService : ICrudService<OrderDto, CreateOrderDto, UpdateOrderDto, PatchOrderDto> { }
    public interface IOrderItemService : ICrudService<OrderItemDto, CreateOrderItemDto, UpdateOrderItemDto, PatchOrderItemDto> { }

    // ---------- Role ----------
    public class RoleService
        : CrudService<Role, RoleDto, CreateRoleDto, UpdateRoleDto, PatchRoleDto>, IRoleService
    {
        public RoleService(IRepository<Role> repository, IMapper mapper) : base(repository, mapper) { }
    }

    // ---------- Category ----------
    public class CategoryService
        : CrudService<Category, CategoryDto, CreateCategoryDto, UpdateCategoryDto, PatchCategoryDto>, ICategoryService
    {
        public CategoryService(IRepository<Category> repository, IMapper mapper) : base(repository, mapper) { }
    }

    // ---------- User ----------
    public class UserService
        : CrudService<User, UserDto, CreateUserDto, UpdateUserDto, PatchUserDto>, IUserService
    {
        private readonly IUserRepository _users;
        private readonly IRoleRepository _roles;
        private readonly IPasswordHasher<User> _hasher;

        public UserService(
            IUserRepository users, IRoleRepository roles,
            IPasswordHasher<User> hasher, IMapper mapper)
            : base(users, mapper)
        {
            _users = users;
            _roles = roles;
            _hasher = hasher;
        }

        // ---------- Регистрация и вход (шаг 2 методички) ----------

        public async Task<User> RegisterAsync(string login, string password, CancellationToken ct = default)
        {
            login = login.Trim();
            await EnsureLoginFreeAsync(login, null, ct);

            // Через регистрацию можно получить только обычную роль; остальные роли выдаёт администратор.
            var role = await _roles.FindByNameAsync(AppRoles.User, ct)
                ?? throw new InvalidOperationException(
                    $"В таблице Roles нет роли «{AppRoles.User}». Выполните SQL-скрипт add_auth.sql.");

            var user = new User { Login = login, Role_Id = role.Role_Id, Role = role, IsActive = true };
            user.PasswordHash = _hasher.HashPassword(user, password);

            await _users.AddAsync(user, ct);
            await _users.SaveChangesAsync(ct);
            return user;
        }

        public async Task<User?> ValidateCredentialsAsync(string login, string password, CancellationToken ct = default)
        {
            var user = await _users.FindByLoginAsync(login.Trim(), ct);
            if (user is null || !user.IsActive) return null;

            PasswordVerificationResult result;
            try
            {
                result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            }
            catch (FormatException)
            {
                // В PasswordHash лежит не хеш (например, старый пароль открытым текстом) — вход невозможен
                return null;
            }

            if (result == PasswordVerificationResult.Failed) return null;

            // Хеш создан по устаревшим параметрам — обновляем его при успешном входе
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _hasher.HashPassword(user, password);
                await _users.SaveChangesAsync(ct);
            }

            return user;
        }

        // Пароль приходит открытым текстом только во входном DTO; в базу уходит хеш.
        protected override void AfterMapCreate(User entity, CreateUserDto dto) =>
            entity.PasswordHash = _hasher.HashPassword(entity, dto.Password);

        protected override void AfterMapUpdate(User entity, UpdateUserDto dto) =>
            entity.PasswordHash = _hasher.HashPassword(entity, dto.Password);

        protected override void AfterMapPatch(User entity, PatchUserDto dto)
        {
            if (dto.Password is not null)
                entity.PasswordHash = _hasher.HashPassword(entity, dto.Password);
        }

        protected override async Task ValidateCreateAsync(CreateUserDto dto, CancellationToken ct)
        {
            await EnsureExistsAsync(_roles, dto.Role_Id, "Роль", ct);
            await EnsureLoginFreeAsync(dto.Login, null, ct);
        }

        protected override async Task ValidateUpdateAsync(int id, UpdateUserDto dto, CancellationToken ct)
        {
            await EnsureExistsAsync(_roles, dto.Role_Id, "Роль", ct);
            await EnsureLoginFreeAsync(dto.Login, id, ct);
        }

        protected override async Task ValidatePatchAsync(int id, PatchUserDto dto, CancellationToken ct)
        {
            if (dto.Role_Id is not null)
                await EnsureExistsAsync(_roles, dto.Role_Id.Value, "Роль", ct);
            if (dto.Login is not null)
                await EnsureLoginFreeAsync(dto.Login, id, ct);
        }

        private async Task EnsureLoginFreeAsync(string login, int? excludeId, CancellationToken ct)
        {
            if (await _users.LoginExistsAsync(login, excludeId, ct))
                throw new ConflictException($"Логин «{login}» уже занят.");
        }
    }

    // ---------- Product ----------
    public class ProductService
        : CrudService<Product, ProductDto, CreateProductDto, UpdateProductDto, PatchProductDto>, IProductService
    {
        private readonly IRepository<Category> _categories;

        public ProductService(IRepository<Product> repository, IRepository<Category> categories, IMapper mapper)
            : base(repository, mapper)
        {
            _categories = categories;
        }

        protected override Task ValidateCreateAsync(CreateProductDto dto, CancellationToken ct) =>
            EnsureExistsAsync(_categories, dto.Category_Id, "Категория", ct);

        protected override Task ValidateUpdateAsync(int id, UpdateProductDto dto, CancellationToken ct) =>
            EnsureExistsAsync(_categories, dto.Category_Id, "Категория", ct);

        protected override Task ValidatePatchAsync(int id, PatchProductDto dto, CancellationToken ct) =>
            dto.Category_Id is null
                ? Task.CompletedTask
                : EnsureExistsAsync(_categories, dto.Category_Id.Value, "Категория", ct);
    }

    // ---------- Cart ----------
    public class CartService
        : CrudService<Cart, CartDto, CreateCartDto, UpdateCartDto, PatchCartDto>, ICartService
    {
        private readonly IUserRepository _users;

        public CartService(IRepository<Cart> repository, IUserRepository users, IMapper mapper)
            : base(repository, mapper)
        {
            _users = users;
        }

        protected override Task ValidateCreateAsync(CreateCartDto dto, CancellationToken ct) =>
            EnsureExistsAsync(_users, dto.User_Id, "Пользователь", ct);

        protected override Task ValidateUpdateAsync(int id, UpdateCartDto dto, CancellationToken ct) =>
            EnsureExistsAsync(_users, dto.User_Id, "Пользователь", ct);

        protected override Task ValidatePatchAsync(int id, PatchCartDto dto, CancellationToken ct) =>
            dto.User_Id is null
                ? Task.CompletedTask
                : EnsureExistsAsync(_users, dto.User_Id.Value, "Пользователь", ct);
    }

    // ---------- CartItem ----------
    public class CartItemService
        : CrudService<CartItem, CartItemDto, CreateCartItemDto, UpdateCartItemDto, PatchCartItemDto>, ICartItemService
    {
        private readonly IRepository<Cart> _carts;
        private readonly IRepository<Product> _products;

        public CartItemService(
            IRepository<CartItem> repository, IRepository<Cart> carts,
            IRepository<Product> products, IMapper mapper)
            : base(repository, mapper)
        {
            _carts = carts;
            _products = products;
        }

        protected override async Task ValidateCreateAsync(CreateCartItemDto dto, CancellationToken ct)
        {
            await EnsureExistsAsync(_carts, dto.Cart_Id, "Корзина", ct);
            await EnsureExistsAsync(_products, dto.Product_Id, "Товар", ct);
        }

        protected override async Task ValidateUpdateAsync(int id, UpdateCartItemDto dto, CancellationToken ct)
        {
            await EnsureExistsAsync(_carts, dto.Cart_Id, "Корзина", ct);
            await EnsureExistsAsync(_products, dto.Product_Id, "Товар", ct);
        }

        protected override async Task ValidatePatchAsync(int id, PatchCartItemDto dto, CancellationToken ct)
        {
            if (dto.Cart_Id is not null)
                await EnsureExistsAsync(_carts, dto.Cart_Id.Value, "Корзина", ct);
            if (dto.Product_Id is not null)
                await EnsureExistsAsync(_products, dto.Product_Id.Value, "Товар", ct);
        }
    }

    // ---------- Order ----------
    public class OrderService
        : CrudService<Order, OrderDto, CreateOrderDto, UpdateOrderDto, PatchOrderDto>, IOrderService
    {
        private readonly IUserRepository _users;

        public OrderService(IRepository<Order> repository, IUserRepository users, IMapper mapper)
            : base(repository, mapper)
        {
            _users = users;
        }

        protected override Task ValidateCreateAsync(CreateOrderDto dto, CancellationToken ct) =>
            EnsureExistsAsync(_users, dto.User_Id, "Пользователь", ct);

        protected override Task ValidateUpdateAsync(int id, UpdateOrderDto dto, CancellationToken ct) =>
            EnsureExistsAsync(_users, dto.User_Id, "Пользователь", ct);

        protected override Task ValidatePatchAsync(int id, PatchOrderDto dto, CancellationToken ct) =>
            dto.User_Id is null
                ? Task.CompletedTask
                : EnsureExistsAsync(_users, dto.User_Id.Value, "Пользователь", ct);
    }

    // ---------- OrderItem ----------
    public class OrderItemService
        : CrudService<OrderItem, OrderItemDto, CreateOrderItemDto, UpdateOrderItemDto, PatchOrderItemDto>, IOrderItemService
    {
        private readonly IRepository<Order> _orders;
        private readonly IRepository<Product> _products;

        public OrderItemService(
            IRepository<OrderItem> repository, IRepository<Order> orders,
            IRepository<Product> products, IMapper mapper)
            : base(repository, mapper)
        {
            _orders = orders;
            _products = products;
        }

        protected override async Task ValidateCreateAsync(CreateOrderItemDto dto, CancellationToken ct)
        {
            await EnsureExistsAsync(_orders, dto.Order_Id, "Заказ", ct);
            await EnsureExistsAsync(_products, dto.Product_Id, "Товар", ct);
        }

        protected override async Task ValidateUpdateAsync(int id, UpdateOrderItemDto dto, CancellationToken ct)
        {
            await EnsureExistsAsync(_orders, dto.Order_Id, "Заказ", ct);
            await EnsureExistsAsync(_products, dto.Product_Id, "Товар", ct);
        }

        protected override async Task ValidatePatchAsync(int id, PatchOrderItemDto dto, CancellationToken ct)
        {
            if (dto.Order_Id is not null)
                await EnsureExistsAsync(_orders, dto.Order_Id.Value, "Заказ", ct);
            if (dto.Product_Id is not null)
                await EnsureExistsAsync(_products, dto.Product_Id.Value, "Товар", ct);
        }
    }
}
