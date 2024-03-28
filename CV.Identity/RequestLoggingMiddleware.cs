using System.Text;

namespace CV.Identity
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering(); // This allows us to read the stream more than once

            // Temporarily read the request body
            var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
            await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            var requestBody = Encoding.UTF8.GetString(buffer);
            _logger.LogInformation($"Request body: {requestBody}");

            // Reset the request body stream position so the next middleware can read it
            context.Request.Body.Position = 0;

            await _next(context);
        }
    }

}
