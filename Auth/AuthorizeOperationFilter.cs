using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace pr9ASP.Auth
{
    /// <summary>
    /// Добавляет в Swagger замок (требование схемы Bearer) только тем методам,
    /// у которых реально есть [Authorize] и нет [AllowAnonymous].
    /// Открытые методы остаются без замка.
    /// </summary>
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // EndpointMetadata включает атрибуты и действия, и контроллера
            var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata
                           ?? new List<object>();

            var requiresAuth = metadata.OfType<IAuthorizeData>().Any();
            var allowsAnonymous = metadata.OfType<IAllowAnonymous>().Any();
            if (!requiresAuth || allowsAnonymous) return;

            operation.Responses.TryAdd("401", new OpenApiResponse
            {
                Description = "Не авторизован: токена нет или он недействителен/просрочен"
            });
            operation.Responses.TryAdd("403", new OpenApiResponse
            {
                Description = "Нет прав: не та роль или чужой ресурс"
            });

            var bearer = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement { [bearer] = Array.Empty<string>() }
            };
        }
    }
}
