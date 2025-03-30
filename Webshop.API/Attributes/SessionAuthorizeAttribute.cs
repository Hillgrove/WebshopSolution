using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Webshop.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string[] Roles { get; set; } = Array.Empty<string>();

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;

            var userId = session.GetInt32("UserId");
            var userRole = session.GetString("UserRole");

            if (string.IsNullOrEmpty(userRole))
            {
                context.Result = new UnauthorizedResult(); // 401
                return;
            }

            if (Roles.Length > 0 && !Roles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult();
                return;
            }

            await Task.CompletedTask;
        }
    }
}
