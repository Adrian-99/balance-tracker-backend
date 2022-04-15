namespace API.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate next;
        private ILogger logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            logger.LogInformation($"Request [{context.Request.Method} {context.Request.Path}] received");
            await next.Invoke(context);
            logger.LogInformation($"Request [{context.Request.Method} {context.Request.Path}] finished with code {context.Response.StatusCode}");
        }
    }
}
