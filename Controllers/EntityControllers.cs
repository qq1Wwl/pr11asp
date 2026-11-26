using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pr9ASP.Auth;
using pr9ASP.DTOs;
using pr9ASP.Services;

namespace pr9ASP.Controllers
{
    // Маршруты сохранены из прошлой работы. Политика доступа задана атрибутами у каждого действия:
    //   без атрибута            — открыто всем;
    //   [Authorize]             — нужен валидный JWT (для записей с владельцем дополнительно
    //                             проверяется владелец через политику OwnerOrAdmin);
    //   [Authorize(AdminOnly)]  — только роль Admin (role-based политика);
    //   [Authorize(CanManageProducts)] — роли Admin или Manager (политика с несколькими ролями).

    [Route("api/roles")]
    [Authorize(Policy = Policies.AdminOnly)]
    public class RolesController
        : CrudController<RoleDto, CreateRoleDto, UpdateRoleDto, PatchRoleDto>
    {
        public RolesController(IRoleService service) : base(service, d => d.Role_Id) { }

        [HttpGet]
        public Task<IActionResult> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default) =>
            GetAllCore(page, pageSize, cancellationToken);

        [HttpGet("{id:int}")]
        public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
            GetByIdCore(id, cancellationToken);

        [HttpPost]
        public Task<IActionResult> Create([FromBody] CreateRoleDto dto, CancellationToken cancellationToken) =>
            CreateCore(dto, cancellationToken);

        [HttpPut("{id:int}")]
        public Task<IActionResult> Update(int id, [FromBody] UpdateRoleDto dto, CancellationToken cancellationToken) =>
            UpdateCore(id, dto, cancellationToken);

        [HttpPatch("{id:int}")]
        public Task<IActionResult> Patch(int id, [FromBody] PatchRoleDto dto, CancellationToken cancellationToken) =>
            PatchCore(id, dto, cancellationToken);

        [HttpDelete("{id:int}")]
        public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
            DeleteCore(id, cancellationToken);
    }

    [Route("api/users")]
    public class UsersController
        : OwnedCrudController<UserDto, CreateUserDto, UpdateUserDto, PatchUserDto>
    {
        public UsersController(IUserService service, IAuthorizationService authorization)
            : base(service, d => d.User_Id, d => d.User_Id, authorization) { }

        [HttpGet, Authorize(Policy = Policies.AdminOnly)]
        public Task<IActionResult> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default) =>
            GetAllCore(page, pageSize, cancellationToken);

        [HttpGet("{id:int}"), Authorize]
        public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
            GetByIdCore(id, cancellationToken);

        [HttpPost, Authorize(Policy = Policies.AdminOnly)]
        public Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken) =>
            CreateCore(dto, cancellationToken);

        [HttpPut("{id:int}"), Authorize(Policy = Policies.AdminOnly)]
        public Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken) =>
            UpdateCore(id, dto, cancellationToken);

        [HttpPatch("{id:int}"), Authorize(Policy = Policies.AdminOnly)]
        public Task<IActionResult> Patch(int id, [FromBody] PatchUserDto dto, CancellationToken cancellationToken) =>
            PatchCore(id, dto, cancellationToken);

        [HttpDelete("{id:int}"), Authorize(Policy = Policies.AdminOnly)]
        public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
            DeleteCore(id, cancellationToken);

        // Профиль текущего пользователя: id берётся из токена
        [HttpGet("me"), Authorize]
        public async Task<IActionResult> Me(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            return await GetByIdCore(userId.Value, cancellationToken);
        }
    }

    [Route("api/categories")]
    public class CategoriesController
        : CrudController<CategoryDto, CreateCategoryDto, UpdateCategoryDto, PatchCategoryDto>
    {
        public CategoriesController(ICategoryService service) : base(service, d => d.Category_Id) { }

        [HttpGet]
        public Task<IActionResult> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default) =>
            GetAllCore(page, pageSize, cancellationToken);

        [HttpGet("{id:int}")]
        public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
            GetByIdCore(id, cancellationToken);

        [HttpPost, Authorize(Policy = Policies.CanManageProducts)]
        public Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken cancellationToken) =>
            CreateCore(dto, cancellationToken);

        [HttpPut("{id:int}"), Authorize(Policy = Policies.CanManageProducts)]
        public Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto, CancellationToken cancellationToken) =>
            UpdateCore(id, dto, cancellationToken);

        [HttpPatch("{id:int}"), Authorize(Policy = Policies.CanManageProducts)]
        public Task<IActionResult> Patch(int id, [FromBody] PatchCategoryDto dto, CancellationToken cancellationToken) =>
            PatchCore(id, dto, cancellationToken);

        [HttpDelete("{id:int}"), Authorize(Policy = Policies.AdminOnly)]
        public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
            DeleteCore(id, cancellationToken);
    }

    [Route("api/products")]
    public class ProductsController
        : CrudController<ProductDto, CreateProductDto, UpdateProductDto, PatchProductDto>
    {
        public ProductsController(IProductService service) : base(service, d => d.Product_Id) { }

        [HttpGet]
        public Task<IActionResult> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default) =>
            GetAllCore(page, pageSize, cancellationToken);

        [HttpGet("{id:int}")]
        public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
            GetByIdCore(id, cancellationToken);

        [HttpPost, Authorize(Policy = Policies.CanManageProducts)]
        public Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken) =>
            CreateCore(dto, cancellationToken);

        [HttpPut("{id:int}"), Authorize(Policy = Policies.CanManageProducts)]
        public Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken) =>
            UpdateCore(id, dto, cancellationToken);

        [HttpPatch("{id:int}"), Authorize(Policy = Policies.CanManageProducts)]
        public Task<IActionResult> Patch(int id, [FromBody] PatchProductDto dto, CancellationToken cancellationToken) =>
            PatchCore(id, dto, cancellationToken);

        [HttpDelete("{id:int}"), Authorize(Policy = Policies.AdminOnly)]
        public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
            DeleteCore(id, cancellationToken);
    }

    [Route("api/carts")]
    public class CartsController
        : OwnedCrudController<CartDto, CreateCartDto, UpdateCartDto, PatchCartDto>
    {
        public CartsController(ICartService service, IAuthorizationService authorization)
            : base(service, d => d.Cart_Id, d => d.User_Id, authorization) { }

        [HttpGet, Authorize(Policy = Policies.AdminOnly)]
        public Task<IActionResult> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default) =>
            GetAllCore(page, pageSize, cancellationToken);

        // Только записи текущего пользователя (id берётся из токена)
        [HttpGet("my"), Authorize]
        public Task<IActionResult> GetMy(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default) =>
            GetMyCore(page, pageSize, cancellationToken);

        [HttpGet("{id:int}"), Authorize]
        public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
            GetByIdCore(id, cancellationToken);

        [HttpPost, Authorize]
        public Task<IActionResult> Create([FromBody] CreateCartDto dto, CancellationToken cancellationToken) =>
            CreateCore(dto, cancellationToken);

        [HttpPut("{id:int}"), Authorize]
        public Task<IActionResult> Update(int id, [FromBody] UpdateCartDto dto, CancellationToken cancellationToken) =>
            UpdateCore(id, dto, cancellationToken);

        [HttpPatch("{id:int}"), Authorize]
        public Task<IActionResult> Patch(int id, [FromBody] PatchCartDto dto, CancellationToken cancellationToken) =>
            PatchCore(id, dto, cancellationToken);

        [HttpDelete("{id:int}"), Authorize]
        public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
            DeleteCore(id, cancellationToken);

        // Обычный пользователь работает только со своими записями: User_Id подставляется из токена.
        // Администратор может указать любого пользователя.
        protected override IActionResult? PrepareCreate(CreateCartDto dto)
        {
            if (User.IsAdmin()) return null;
            var me = User.GetUserId();
            if (me is null) return Unauthorized();
            dto.User_Id = me.Value;
            return null;
        }

        protected override IActionResult? PrepareUpdate(UpdateCartDto dto)
        {
            if (User.IsAdmin()) return null;
            var me = User.GetUserId();
            if (me is null) return Unauthorized();
            dto.User_Id = me.Value;
            return null;
        }

        // Передать запись другому пользователю может только администратор
        protected override IActionResult? PreparePatch(PatchCartDto dto)
        {
            if (User.IsAdmin() || dto.User_Id is null) return null;
            return dto.User_Id == User.GetUserId() ? null : Forbid();
        }
    }

    [Route("api/cartitems")]
    [Authorize(Policy = Policies.AdminOnly)]
    public class CartItemsController
        : CrudController<CartItemDto, CreateCartItemDto, UpdateCartItemDto, PatchCartItemDto>
    {
        public CartItemsController(ICartItemService service) : base(service, d => d.CartItem_Id) { }

        [HttpGet]
        public Task<IActionResult> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default) =>
            GetAllCore(page, pageSize, cancellationToken);

        [HttpGet("{id:int}")]
        public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
            GetByIdCore(id, cancellationToken);

        [HttpPost]
        public Task<IActionResult> Create([FromBody] CreateCartItemDto dto, CancellationToken cancellationToken) =>
            CreateCore(dto, cancellationToken);

        [HttpPut("{id:int}")]
        public Task<IActionResult> Update(int id, [FromBody] UpdateCartItemDto dto, CancellationToken cancellationToken) =>
            UpdateCore(id, dto, cancellationToken);

        [HttpPatch("{id:int}")]
        public Task<IActionResult> Patch(int id, [FromBody] PatchCartItemDto dto, CancellationToken cancellationToken) =>
            PatchCore(id, dto, cancellationToken);

        [HttpDelete("{id:int}")]
        public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
            DeleteCore(id, cancellationToken);
    }

    [Route("api/orders")]
    public class OrdersController
        : OwnedCrudController<OrderDto, CreateOrderDto, UpdateOrderDto, PatchOrderDto>
    {
        public OrdersController(IOrderService service, IAuthorizationService authorization)
            : base(service, d => d.Order_Id, d => d.User_Id, authorization) { }

        [HttpGet, Authorize(Policy = Policies.AdminOnly)]
        public Task<IActionResult> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default) =>
            GetAllCore(page, pageSize, cancellationToken);

        // Только записи текущего пользователя (id берётся из токена)
        [HttpGet("my"), Authorize]
        public Task<IActionResult> GetMy(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default) =>
            GetMyCore(page, pageSize, cancellationToken);

        [HttpGet("{id:int}"), Authorize]
        public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
            GetByIdCore(id, cancellationToken);

        [HttpPost, Authorize]
        public Task<IActionResult> Create([FromBody] CreateOrderDto dto, CancellationToken cancellationToken) =>
            CreateCore(dto, cancellationToken);

        [HttpPut("{id:int}"), Authorize]
        public Task<IActionResult> Update(int id, [FromBody] UpdateOrderDto dto, CancellationToken cancellationToken) =>
            UpdateCore(id, dto, cancellationToken);

        [HttpPatch("{id:int}"), Authorize]
        public Task<IActionResult> Patch(int id, [FromBody] PatchOrderDto dto, CancellationToken cancellationToken) =>
            PatchCore(id, dto, cancellationToken);

        [HttpDelete("{id:int}"), Authorize]
        public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
            DeleteCore(id, cancellationToken);

        // Обычный пользователь работает только со своими записями: User_Id подставляется из токена.
        // Администратор может указать любого пользователя.
        protected override IActionResult? PrepareCreate(CreateOrderDto dto)
        {
            if (User.IsAdmin()) return null;
            var me = User.GetUserId();
            if (me is null) return Unauthorized();
            dto.User_Id = me.Value;
            return null;
        }

        protected override IActionResult? PrepareUpdate(UpdateOrderDto dto)
        {
            if (User.IsAdmin()) return null;
            var me = User.GetUserId();
            if (me is null) return Unauthorized();
            dto.User_Id = me.Value;
            return null;
        }

        // Передать запись другому пользователю может только администратор
        protected override IActionResult? PreparePatch(PatchOrderDto dto)
        {
            if (User.IsAdmin() || dto.User_Id is null) return null;
            return dto.User_Id == User.GetUserId() ? null : Forbid();
        }
    }

    [Route("api/orderitems")]
    [Authorize(Policy = Policies.AdminOnly)]
    public class OrderItemsController
        : CrudController<OrderItemDto, CreateOrderItemDto, UpdateOrderItemDto, PatchOrderItemDto>
    {
        public OrderItemsController(IOrderItemService service) : base(service, d => d.OrderItem_Id) { }

        [HttpGet]
        public Task<IActionResult> GetAll(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default) =>
            GetAllCore(page, pageSize, cancellationToken);

        [HttpGet("{id:int}")]
        public Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
            GetByIdCore(id, cancellationToken);

        [HttpPost]
        public Task<IActionResult> Create([FromBody] CreateOrderItemDto dto, CancellationToken cancellationToken) =>
            CreateCore(dto, cancellationToken);

        [HttpPut("{id:int}")]
        public Task<IActionResult> Update(int id, [FromBody] UpdateOrderItemDto dto, CancellationToken cancellationToken) =>
            UpdateCore(id, dto, cancellationToken);

        [HttpPatch("{id:int}")]
        public Task<IActionResult> Patch(int id, [FromBody] PatchOrderItemDto dto, CancellationToken cancellationToken) =>
            PatchCore(id, dto, cancellationToken);

        [HttpDelete("{id:int}")]
        public Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
            DeleteCore(id, cancellationToken);
    }
}
