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

            // Skip Content-Type validation for logout
            if (path == "/api/users/logout")
            {
                await _next(context);
                return;
            }

            // Skip Content-Type check for GET requests
            if (context.Request.Method == HttpMethods.Get)
            {
                await _next(context);
                return;
            }

            if (context.Request.Method == "POST" && context.Request.ContentLength == 0)
            {   
                // Allow POST request without body
                // TODO: Double check if this is best practice
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
