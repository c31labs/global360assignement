using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Api.Middleware;

/// <summary>
/// Centralised exception → <see cref="ProblemDetails"/> translator. Keeps endpoint code
/// focused on the happy path; gives clients consistent, machine-readable errors (RFC 7807).
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
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
            await _next(context).ConfigureAwait(false);
        }
        catch (ValidationException ex)
        {
            _logger.LogInformation(ex, "Request validation failed for {Path}", context.Request.Path);
            await WriteValidationProblem(context, ex).ConfigureAwait(false);
        }
        catch (TaskNotFoundException ex)
        {
            _logger.LogInformation(ex, "Resource not found: {Message}", ex.Message);
            await WriteProblem(context, StatusCodes.Status404NotFound, "Resource not found", ex.Message).ConfigureAwait(false);
        }
        catch (DomainValidationException ex)
        {
            _logger.LogInformation(ex, "Domain rule violated: {Message}", ex.Message);
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Invalid request", ex.Message).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected; nothing to log loudly.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            var detail = _env.IsDevelopment() ? ex.ToString() : "An unexpected error occurred.";
            await WriteProblem(context, StatusCodes.Status500InternalServerError, "Internal server error", detail).ConfigureAwait(false);
        }
    }

    private static async Task WriteValidationProblem(HttpContext context, ValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var problem = new ValidationProblemDetails(errors)
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Instance = context.Request.Path,
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(context.Response.Body, problem, JsonOptions).ConfigureAwait(false);
    }

    private static async Task WriteProblem(HttpContext context, int status, string title, string detail)
    {
        var problem = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{status}",
            Title = title,
            Status = status,
            Detail = detail,
            Instance = context.Request.Path,
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(context.Response.Body, problem, JsonOptions).ConfigureAwait(false);
    }
}
