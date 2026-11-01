using System.Net;
using System.Text.Json;

namespace CoffeeNChill.Api.Middleware;

/// <summary>Catches unhandled exceptions and returns a clean JSON error.</summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _log;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var body = JsonSerializer.Serialize(new
            {
                status = 500,
                error = "An unexpected error occurred.",
                traceId = context.TraceIdentifier
            });
            await context.Response.WriteAsync(body);
        }
    }
}