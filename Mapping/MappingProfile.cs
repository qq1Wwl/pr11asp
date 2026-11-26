using AutoMapper;
using pr9ASP.DTOs;
using pr9ASP.Models;

namespace pr9ASP.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ---------- Entity -> DTO (чтение, с вложенными данными) ----------
            // Вложенные объекты (Role, Category, User, Product, Items) AutoMapper
            // подхватывает по именам свойств; PasswordHash в UserDto не попадает.
            CreateMap<Role, RoleDto>();
            CreateMap<User, UserDto>();
            CreateMap<User, UserShortDto>();
            CreateMap<Category, CategoryDto>();
            CreateMap<Product, ProductDto>();
            CreateMap<Product, ProductShortDto>();
            CreateMap<Cart, CartDto>();
            CreateMap<CartItem, CartItemDto>();
            CreateMap<Order, OrderDto>();
            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<Reservation, ReservationDto>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.ReleasedAt == null));

            // ---------- DTO -> Entity (запись) ----------
            // В DTO только скалярные поля и Id связей. Навигационные свойства
            // (Role, Category, ...) не маппятся — связь задаётся внешним ключом,
            // а его существование проверяет сервис. CreatedAt/OrderDate/ReservedAt
            // заполняются значением по умолчанию в модели (DateTime.UtcNow).

            // Create / Update (PUT)
            CreateMap<CreateRoleDto, Role>();
            CreateMap<UpdateRoleDto, Role>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();
            CreateMap<CreateCartDto, Cart>();
            CreateMap<UpdateCartDto, Cart>();
            CreateMap<CreateCartItemDto, CartItem>();
            CreateMap<UpdateCartItemDto, CartItem>();
            CreateMap<CreateOrderDto, Order>();
            CreateMap<UpdateOrderDto, Order>();
            CreateMap<CreateOrderItemDto, OrderItem>();
            CreateMap<UpdateOrderItemDto, OrderItem>();
            CreateMap<ReserveProductDto, Reservation>();

            // Patch (PATCH): копируем только те поля, которые пришли (не null)
            CreateMap<PatchRoleDto, Role>().ForAllMembers(o => o.Condition((s, d, m) => m != null));
            CreateMap<PatchUserDto, User>().ForAllMembers(o => o.Condition((s, d, m) => m != null));
            CreateMap<PatchCategoryDto, Category>().ForAllMembers(o => o.Condition((s, d, m) => m != null));
            CreateMap<PatchProductDto, Product>().ForAllMembers(o => o.Condition((s, d, m) => m != null));
            CreateMap<PatchCartDto, Cart>().ForAllMembers(o => o.Condition((s, d, m) => m != null));
            CreateMap<PatchCartItemDto, CartItem>().ForAllMembers(o => o.Condition((s, d, m) => m != null));
            CreateMap<PatchOrderDto, Order>().ForAllMembers(o => o.Condition((s, d, m) => m != null));
            CreateMap<PatchOrderItemDto, OrderItem>().ForAllMembers(o => o.Condition((s, d, m) => m != null));
        }
    }
}
