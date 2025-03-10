namespace Webshop.API.Middleware
{
    public class CsrfMiddleware
    {
        private readonly RequestDelegate _next;

        public CsrfMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip CSRF validation for safe methods (GET, OPTIONS)
            if (context.Request.Method == HttpMethods.Get || context.Request.Method == HttpMethods.Options)
            {
                await _next(context);
                return;
            }

            // Skip CSRF validation for authentication-related actions
            var path = context.Request.Path.ToString().ToLower();
            if (path == "/api/users/login" || path == "/api/users/forgot-password" || path == "/api/users/reset-password")
            {
                await _next(context);
                return;
            }

            // Extract CSRF cookie and header
            var csrfCookie = context.Request.Cookies["csrf-token"];
            var csrfHeader = context.Request.Headers["X-CSRF-Token"].ToString();

            Console.WriteLine($"CSRF Middleware - Received Header: {csrfHeader}");
            Console.WriteLine($"CSRF Middleware - Received Cookie: {csrfCookie}");

            if (string.IsNullOrEmpty(csrfCookie) || csrfCookie != csrfHeader)
            {
                Console.WriteLine("CSRF validation failed: cokie and header do not match.");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("CSRF validation failed");
                return;
            }

            Console.WriteLine("CSRF validation passed.");
            await _next(context);
        }
    }
}
