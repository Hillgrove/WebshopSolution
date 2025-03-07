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

            // Skip CSRF check for login (since the token is not yet available)
            var path = context.Request.Path.ToString().ToLower();
            if (path == "/api/users/login")
            {
                await _next(context);
                return;
            }

            // Extract CSRF token from cookie and header
            var csrfCookie = context.Request.Cookies["csrf-token"];
            //var csrfHeader = context.Request.Headers["X-CSRF-Token"].FirstOrDefault();

            //Console.WriteLine($"CSRF Check - Cookie: {csrfCookie}, Header: {csrfHeader}");

            //if (string.IsNullOrEmpty(csrfCookie) || csrfCookie != csrfHeader)
            if (string.IsNullOrEmpty(csrfCookie))
            {
                Console.WriteLine($"CSRF validation failed: Cookie missing");
                //Console.WriteLine($"CSRF validation failed. Cookie: {csrfCookie}, Header: {csrfHeader}");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("CSRF validation failed");
                return;
            }

            Console.WriteLine("CSRF validation passed.");
            await _next(context);
        }
    }
}
