using Application.ApplicationExceptions;
using Domain.DomainExceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace API.Middleware;


public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
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

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        HttpStatusCode status;
        string detail = ex.Message;

        switch (ex)
        {
            case NotFoundException:
                status = HttpStatusCode.NotFound;
                break;
            case ConflictException:
                status = HttpStatusCode.Conflict;
                break;
            case DomainValidationException:
                status = HttpStatusCode.BadRequest;
                break;
            default:
                status = HttpStatusCode.InternalServerError;
                detail = "An unexpected error occurred.";
                break;
        }

        var problemDetails = new ProblemDetails
        {
            Status = (int)status,
            Title = status.ToString(),
            Detail = detail,
            Type = $"https://httpstatuses.com/{(int)status}",
            Instance = context.Request.Path
        };

        // Ajout du traceId pour debug serveur
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
