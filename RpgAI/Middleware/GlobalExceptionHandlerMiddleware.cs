namespace RpgAI.Middleware;

using System.Net;
using System.Text.Json;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError(exception, "An unexpected error occurred: {Message}", exception.Message);

        context.Response.ContentType = "application/json";

        HttpStatusCode statusCode;
        string title;
        string detail = exception.Message;

        switch (exception)
        {
            case NotFoundException:
                statusCode = HttpStatusCode.NotFound;
                title = "Resource Not Found";
                break;
            case AlreadyExistsException:
                statusCode = HttpStatusCode.Conflict;
                title = "Resource Already Exists";
                break;
            default:
                statusCode = HttpStatusCode.InternalServerError;
                title = "An unexpected error occurred";
                detail = "An error occurred while processing your request.";
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        ProblemDetails problemDetails = new()
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail
        };

        string jsonResponse = JsonSerializer.Serialize(problemDetails);

        await context.Response.WriteAsync(jsonResponse);
    }
}
