using System.Diagnostics;

namespace TodoApi.Middleware;

public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public PerformanceMonitoringMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMonitoringMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            var statusCode = context.Response.StatusCode;

            // Log slow requests (> 500ms as per requirements)
            if (elapsedMs > 500)
            {
                _logger.LogWarning(
                    "Slow request detected: {Method} {Path} took {ElapsedMs}ms (Status: {StatusCode})",
                    method, path, elapsedMs, statusCode);
            }
            else if (elapsedMs > 1000)
            {
                _logger.LogError(
                    "Very slow request detected: {Method} {Path} took {ElapsedMs}ms (Status: {StatusCode})",
                    method, path, elapsedMs, statusCode);
            }

            // Add performance header
            context.Response.Headers.Append("X-Response-Time", $"{elapsedMs}ms");
        }
    }
}

