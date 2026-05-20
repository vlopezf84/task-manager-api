using System.Net;
using System.Text.Json;
using TaskManagerAPI.Exceptions;

namespace TaskManagerAPI.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Pasa la request al siguiente middleware normalmente
            await _next(context);
        }
        catch (Exception ex)
        {
            // Si algo explota, lo interceptamos aquí
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var response = ex switch
        {
            NotFoundException notFound => new ErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = notFound.Message
            },
            BadRequestException badRequest => new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = badRequest.Message
            },
            _ => new ErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An unexpected error occurred. Please try again later.",
                Detail = _env.IsDevelopment() ? ex.Message : null
            }
        };

        context.Response.StatusCode = response.StatusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}