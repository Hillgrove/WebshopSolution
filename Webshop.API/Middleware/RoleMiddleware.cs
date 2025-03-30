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

            if (!context.Session.Keys.Contains("UserRole"))
            {
                role = "Guest";
                context.Session.SetString("UserRole", role);
                context.Session.SetInt32("UserId", -1);
            }

            await _next(context);
        }
    }

}
