using System.Net;

namespace PosCorte.API.Middleware
{
    public class ErroMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErroMiddleware> _logger;

        public ErroMiddleware(RequestDelegate next, ILogger<ErroMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Erro não tratado na requisição");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = new
            {
                error = "Erro interno do servidor",
                details = exception.Message,
                timestamp = DateTime.UtcNow
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
