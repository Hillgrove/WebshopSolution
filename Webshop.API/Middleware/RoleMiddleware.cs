namespace Webshop.API.Middleware
{
    public class RoleMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var role = context.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role))
            {
                role = "Guest";
                context.Session.SetString("UserRole", role);
            }

            // context.Items["UserRole"] = role;
            await _next(context);
        }
    }

}
