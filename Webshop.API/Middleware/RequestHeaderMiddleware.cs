namespace Webshop.API.Middleware
{
    public class RequestHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _allowedContentTypes = { "application/json", "application/xml" };

        public RequestHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            var contentType = context.Request.Headers.ContentType.ToString().ToLower();

            var path = context.Request.Path.ToString().ToLower();

            // Skip Content-Type validation for valid bodyless POST endpoints
            if (context.Request.Method == "POST" && 
                context.Request.ContentType == null && 
                context.Request.ContentLength == 0 &&
                (path == "/api/users/logout" || path == "/api/cart/checkout"))
            {
                await _next(context);
                return;
            }

            // Skip Content-Type validation for GET and DELETE requests
            if (context.Request.Method == HttpMethods.Get || context.Request.Method == HttpMethods.Delete)
            {
                await _next(context);
                return;
            }

            else if (string.IsNullOrEmpty(contentType) || !_allowedContentTypes.Contains(contentType))
            {
                context.Response.StatusCode = contentType == "" ? 415 : 406; // 415 if no Content-Type, 406 if unsupported type
                context.Response.ContentType = "text/plain"; // Optional, could also be application/json or others
                await context.Response.WriteAsync($"Unsupported Content-Type: {contentType}");
                return;
            }

            await _next(context);
        }
    }

}
