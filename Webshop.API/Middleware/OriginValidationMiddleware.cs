namespace Webshop.API.Middleware
{
    public class OriginValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HashSet<string> _allowedOrigins = new()
    {
        "https://127.0.0.1:5500",
        "https://localhost:5500",
        "https://webshop.hillgrove.dk"
    };

        public OriginValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Post ||
                context.Request.Method == HttpMethods.Put ||
                context.Request.Method == HttpMethods.Delete)
            {
                var origin = context.Request.Headers["Origin"].ToString();
                if (string.IsNullOrEmpty(origin) || !_allowedOrigins.Contains(origin))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Invalid Origin");
                    return;
                }
            }

            await _next(context);
        }
    }

}
