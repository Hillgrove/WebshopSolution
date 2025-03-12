namespace Webshop.API.Middleware
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
            Console.WriteLine($"[{context.TraceIdentifier}] ResponseHeaderMiddleware - Processing response.");

            var headers = context.Response.Headers;

            // Add X-Content-Type-Options header
            if (!headers.ContainsKey("X-Content-Type-Options"))
            {
                headers.Append("X-Content-Type-Options", "nosniff");
            }

            // Add Content-Security-Policy header
            if (!headers.ContainsKey("Content-Security-Policy"))
            {
                headers.Append("Content-Security-Policy",
                                                "default-src 'self'; " +
                                                "script-src 'self'; " +
                                                "object-src 'none';" +
                                                "frame-ancestors 'none';" +
                                                "upgrade-insecure-requests;" +
                                                "base-uri 'self';"
                                                );
            }

            await _next(context);

            if (!headers.ContainsKey("Content-Type"))
            {
                headers.Append("Content-Type", "application/json; charset=utf-8");
            }

            // For text/*, */+xml, and application/xml, ensure the charset is UTF-8
            var contentType = context.Response.ContentType?.ToLower();

            if (contentType != null && (contentType.StartsWith("text/") ||
                                        contentType.Contains("+xml") ||
                                        contentType.Contains("application/xml")))
            {
                if (!contentType.Contains("charset="))
                {
                    headers.ContentType = contentType + "; charset=utf-8";
                }
            }
        }
    }
}
