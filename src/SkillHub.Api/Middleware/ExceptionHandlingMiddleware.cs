using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Common.Exceptions;
using SkillHub.Domain.Common;

namespace SkillHub.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException exception)
        {
            var status = exception.Kind switch
            {
                DomainErrorKind.Conflict => StatusCodes.Status409Conflict,
                DomainErrorKind.Forbidden => StatusCodes.Status403Forbidden,
                DomainErrorKind.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status400BadRequest
            };
            await WriteProblemAsync(context, status, exception.Message);
        }
        catch (AppException exception)
        {
            await WriteProblemAsync(context, exception.StatusCode, exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled error while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError,
                "An unexpected error occurred. Please try again.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
    {
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = TitleFor(statusCode),
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    }

    private static string TitleFor(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Validation error",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not found",
        StatusCodes.Status409Conflict => "Business rule violation",
        _ => "Unexpected error"
    };
}
