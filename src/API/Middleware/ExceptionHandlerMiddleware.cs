using Application.Exceptions;
using Application.Utilities;
using Newtonsoft.Json;

namespace API.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;
        private ILogger logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (ResponseStatusException ex)
            {
                await HandleResponseStatusExceptionAsync(context, ex).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HandleExceptionMessageAsync(context, ex).ConfigureAwait(false);
            }
        }

        private Task HandleExceptionMessageAsync(HttpContext context, Exception exception)
        {
            logger.LogError($"{exception.Message}\n{exception.StackTrace}");

            var result = JsonConvert.SerializeObject(ApiResponse<string>.Error(
                "Internal server error"
                // TODO: Add translation key
                ));
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            return context.Response.WriteAsync(result);
        }

        private Task HandleResponseStatusExceptionAsync(HttpContext context, ResponseStatusException exception)
        {
            logger.LogWarning($"{exception.GetType().Name}: {exception.Message}");

            var result = JsonConvert.SerializeObject(ApiResponse<string>.Error(
                exception.Message,
                exception.ErrorTranslationKey
                ));
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception.StatusCode;

            return context.Response.WriteAsync(result);
        }
    }
}
