using System.Diagnostics;

namespace PosCorte.API.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N")[..8];

            _logger.LogInformation("[{RequestId}] {Method} {Path} iniciado",
                requestId,
                context.Request.Method,
                context.Request.Path);

            await _next(context);

            sw.Stop();

            _logger.LogInformation("[{RequestId}] {Method} {Path} => {StatusCode} em {ElapsedMs}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds);
        }
    }
}
