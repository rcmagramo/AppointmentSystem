using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using AppointmentSystem.Domain.Exceptions;

namespace AppointmentSystem.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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
        _logger.LogError(exception, "An unhandled exception occurred");

        var problemDetails = exception switch
        {
            ValidationException validationEx => CreateValidationProblemDetails(context, validationEx),
            AppointmentNotFoundException notFoundEx => CreateNotFoundProblemDetails(context, notFoundEx),
            DomainException domainEx => CreateDomainProblemDetails(context, domainEx),
            _ => CreateInternalServerErrorProblemDetails(context, exception)
        };

        context.Response.StatusCode = problemDetails.Status ?? 500;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private ProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.BadRequest,
            Instance = context.Request.Path
        };
    }

    private ProblemDetails CreateDomainProblemDetails(
        HttpContext context,
        DomainException exception)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Business rule violation",
            Status = (int)HttpStatusCode.BadRequest,
            Instance = context.Request.Path,
            Detail = exception.Message
        };
    }

    private ProblemDetails CreateNotFoundProblemDetails(
        HttpContext context,
        AppointmentNotFoundException exception)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Resource not found",
            Status = (int)HttpStatusCode.NotFound,
            Instance = context.Request.Path,
            Detail = exception.Message
        };
    }

    private ProblemDetails CreateInternalServerErrorProblemDetails(
        HttpContext context,
        Exception exception)
    {
        var detail = _environment.IsDevelopment()
            ? exception.ToString()
            : "An error occurred while processing your request.";

        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = (int)HttpStatusCode.InternalServerError,
            Instance = context.Request.Path,
            Detail = detail
        };
    }
}