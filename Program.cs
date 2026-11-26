using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using pr9ASP.Auth;
using pr9ASP.Controllers;
using pr9ASP.Data;
using pr9ASP.Models;
using pr9ASP.Repositories;
using pr9ASP.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("ShopDbConnection")));

// ============================================================
// Слои (Controller -> Service -> Repository -> DbContext)
// ============================================================
// ShopDbContext — Scoped (один экземпляр на HTTP-запрос). Repository и Service зависят
// от него, поэтому тоже Scoped:
//  - Singleton не может держать Scoped-зависимость (контекст жил бы вечно и не потокобезопасен);
//  - Transient дал бы каждому репозиторию свой DbContext, и один SaveChangesAsync
//    не сохранил бы изменения сразу двух сущностей (товар + резерв).

// Репозитории
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRepository<Role>>(sp => sp.GetRequiredService<IRoleRepository>());
builder.Services.AddScoped<IRepository<Category>, CategoryRepository>();
builder.Services.AddScoped<IRepository<Product>, ProductRepository>();
builder.Services.AddScoped<IRepository<Cart>, CartRepository>();
builder.Services.AddScoped<IRepository<CartItem>, CartItemRepository>();
builder.Services.AddScoped<IRepository<Order>, OrderRepository>();
builder.Services.AddScoped<IRepository<OrderItem>, OrderItemRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

// Сервисы
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICartItemService, CartItemService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

// AutoMapper подхватывает все классы-наследники Profile в сборке
builder.Services.AddAutoMapper(typeof(Program));

// ============================================================
// Хеширование паролей и JWT
// ============================================================
// PasswordHasher<T> (PBKDF2 + соль, формат хранит параметры) — пароль в открытом виде не хранится.
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.Configure<JwtSettings>(config.GetSection(JwtSettings.SectionName));
builder.Services.AddScoped<ITokenService, TokenService>();

var jwt = config.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
          ?? throw new InvalidOperationException("Не найдена секция Jwt в appsettings.json.");
if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
    throw new InvalidOperationException("Jwt:Key должен быть не короче 32 символов.");

// ============================================================
// Аутентификация: JWT bearer
// ============================================================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Не переименовываем claims: в токене "sub" и "role", так их и читаем
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)), // подпись

            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,                                                      // издатель

            ValidateAudience = true,
            ValidAudience = jwt.Audience,                                                  // аудитория

            ValidateLifetime = true,                                                       // срок действия
            ClockSkew = TimeSpan.Zero,

            NameClaimType = AppClaims.Login,
            RoleClaimType = AppClaims.Role
        };
    });

// ============================================================
// Авторизация: политики
// ============================================================
builder.Services.AddAuthorization(options =>
{
    // Role-based: действие только для роли Admin
    options.AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(AppRoles.Admin));

    // Role-based с несколькими ролями: достаточно любой из перечисленных (Admin или Manager)
    options.AddPolicy(Policies.CanManageProducts, policy =>
        policy.RequireRole(AppRoles.Admin, AppRoles.Manager));

    // Resource-based: владелец ресурса или Admin. Само решение принимает
    // OwnerOrAdminHandler, а вызывается политика из контроллеров через IAuthorizationService.
    options.AddPolicy(Policies.OwnerOrAdmin, policy =>
        policy.RequireAuthenticatedUser().AddRequirements(new OwnerOrAdminRequirement()));
});
builder.Services.AddSingleton<IAuthorizationHandler, OwnerOrAdminHandler>();

// ============================================================
// CORS: только конкретные origin'ы из конфигурации (не AllowAnyOrigin)
// ============================================================
const string FrontendPolicy = "FrontendPolicy";
var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
if (allowedOrigins.Length == 0)
    throw new InvalidOperationException("В appsettings.json не заданы Cors:AllowedOrigins.");

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        // без этого браузер не даст JS прочитать эти заголовки ответа
        .WithExposedHeaders("X-Total-Count", "Retry-After"));
});

// ============================================================
// Rate limiting на входе: не больше N попыток за окно с одного IP, иначе 429
// ============================================================
var loginPermitLimit = config.GetValue("RateLimiting:Login:PermitLimit", 5);
var loginWindowSeconds = config.GetValue("RateLimiting:Login:WindowSeconds", 60);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(AuthController.LoginRateLimitPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = loginPermitLimit,
                Window = TimeSpan.FromSeconds(loginWindowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        var response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            response.Headers["Retry-After"] = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();

        await response.WriteAsJsonAsync(
            new { error = "Слишком много попыток входа. Повторите позже." }, cancellationToken);
    };
});

// ============================================================
// Swagger: кнопка Authorize; замок — только у защищённых методов
// ============================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "pr9ASP API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Вставьте JWT-токен из ответа /api/auth/login (слово Bearer добавлять не нужно)."
    });

    // Замок получают только методы с [Authorize]
    options.OperationFilter<AuthorizeOperationFilter>();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options => options.EnablePersistAuthorization());

// В разработке фронтенд ходит по http://localhost — редирект на https ломал бы CORS-preflight
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

// Порядок middleware (шаг 14 методички):
// 1. CORS — решает, пропускать ли запрос с этого origin; стоит первым, чтобы даже
//    ответы 401/403/429 содержали CORS-заголовки и фронтенд мог их прочитать.
// 2. Аутентификация — кто ты.
// 3. Авторизация — что тебе можно.
// 4. Rate limiter — не слишком ли часто; ему нужно знать, на какой эндпоинт пришёл запрос.
app.UseCors(FrontendPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

// Роли Admin/Manager/User и первый администратор (логин/пароль из секции Seed)
await DbSeeder.SeedAsync(app.Services);

app.Run();
