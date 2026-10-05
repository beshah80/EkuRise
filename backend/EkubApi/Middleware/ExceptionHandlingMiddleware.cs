using System.Net;
using System.Text.Json;

namespace EkubApi.Middleware;

/// <summary>
/// Maps domain exceptions to HTTP responses with appropriate status codes.
/// Keeps controllers slim — no try/catch boilerplate needed in actions.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title) = ex switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, ex.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new { title, status = (int)statusCode };
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
