using Microsoft.AspNetCore.Authorization;

namespace pr9ASP.Auth
{
    /// <summary>
    /// Ресурс, у которого есть владелец. Передаётся в IAuthorizationService.AuthorizeAsync
    /// как "resource": обработчик решает, можно ли текущему пользователю с ним работать.
    /// </summary>
    public record OwnedResource(int OwnerUserId);

    /// <summary>Требование: пользователь — владелец ресурса или администратор.</summary>
    public class OwnerOrAdminRequirement : IAuthorizationRequirement
    {
    }

    /// <summary>
    /// Собственный resource-based обработчик (AuthorizationHandler&lt;TRequirement, TResource&gt;).
    /// Admin — доступно всё; остальные — только ресурсы, где OwnerUserId == Id из токена.
    /// Он вызывается только когда в AuthorizeAsync передан ресурс типа OwnedResource.
    /// </summary>
    public class OwnerOrAdminHandler : AuthorizationHandler<OwnerOrAdminRequirement, OwnedResource>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OwnerOrAdminRequirement requirement,
            OwnedResource resource)
        {
            if (context.User.IsAdmin())
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var currentUserId = context.User.GetUserId();
            if (currentUserId is not null && currentUserId == resource.OwnerUserId)
            {
                context.Succeed(requirement);
            }

            // Ничего не вызываем — требование не выполнено, итог: 403
            return Task.CompletedTask;
        }
    }
}
