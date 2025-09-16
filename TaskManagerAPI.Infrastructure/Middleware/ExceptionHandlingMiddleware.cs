using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TaskManagerAPI.Application.Exceptions;

namespace TaskManagerAPI.Infrastructure.Middleware;

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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unexpected error occurred");

        var (statusCode, title, detail, extensions) = GetProblemDetails(exception);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = GetProblemType(statusCode),
            Extensions = extensions
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(json);
    }

    private (int StatusCode, string Title, string Detail, object? Extensions) GetProblemDetails(Exception exception)
    {
        return exception switch
        {
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Not Found",
                exception.Message,
                null
            ),
            ForbiddenAccessException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                exception.Message,
                null
            ),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "Access denied",
                null
            ),
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                "Validation failed",
                new { errors = validationEx.Errors }
            ),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                exception.Message,
                null
            ),
            InvalidOperationException => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                exception.Message,
                null
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.",
                null
            )
        };
    }

    private string GetProblemType(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
            StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
    }
}