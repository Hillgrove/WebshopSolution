namespace Webshop.API
{
    public class ResponseHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {         
            await _next(context);

            if (!context.Response.Headers.ContainsKey("Content-Type"))
            {
                context.Response.Headers.Append("Content-Type", "application/json; charset=utf-8");
            }

            // For text/*, */+xml, and application/xml, ensure the charset is UTF-8
            var contentType = context.Response.ContentType?.ToLower();

            if (contentType != null && (contentType.StartsWith("text/") ||
                                        contentType.Contains("+xml") ||
                                        contentType.Contains("application/xml")))
            {
                if (!contentType.Contains("charset="))
                {
                    context.Response.Headers["Content-Type"] = contentType + "; charset=utf-8";
                }
            }

            // Add X-Content-Type-Options header
            if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            }

            // Add Content-Security-Policy header
            if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
            {
                context.Response.Headers.Append("Content-Security-Policy",
                                                "default-src 'self'; " +
                                                "script-src 'self'; " +
                                                "object-src 'none';" +
                                                "frame-ancestors 'none';" +
                                                "upgrade-insecure-requests;" + 
                                                "base-uri 'self';"
                                                );
            }
        }
    }

}
