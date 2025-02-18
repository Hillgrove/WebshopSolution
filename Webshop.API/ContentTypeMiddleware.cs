namespace Webshop.API
{
    public class ContentTypeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _allowedContentTypes = { "application/json", "application/xml" };

        public ContentTypeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var contentType = context.Request.Headers["Content-Type"].ToString().ToLower();

            if (string.IsNullOrEmpty(contentType) || !_allowedContentTypes.Contains(contentType))
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
