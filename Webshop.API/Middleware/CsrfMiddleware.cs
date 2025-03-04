using System.IO;

namespace Webshop.API.Middleware
{
    public class CsrfMiddleware
    {
        private readonly RequestDelegate _next;

        public CsrfMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string host = context.Request.Host.ToString().ToLower();

            // Bypass CSRF check ONLY for localhost
            if (host.StartsWith("localhost"))
            {
                await _next(context);
                return;
            }

            // Only validate for state-changing requests
            if (context.Request.Method == "POST" ||
                context.Request.Method == "PUT" ||
                context.Request.Method == "DELETE")
            {
                var headerToken = context.Request.Headers["X-CSRF-Token"].ToString();
                var cookieToken = context.Request.Cookies["XSRF-TOKEN"];

                if (string.IsNullOrEmpty(headerToken) || headerToken != cookieToken)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("CSRF token validation failed.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
