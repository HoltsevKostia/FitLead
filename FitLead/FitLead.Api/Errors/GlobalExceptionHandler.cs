using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FitLead.Api.Errors
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandler(
            IProblemDetailsService problemDetailsService,
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment env)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
            _env = env;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception");

            var (statusCode, errorCode, title) = MapException(exception);

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title
            };

            var antiforgeryException = FindException<AntiforgeryValidationException>(exception);

            if (_env.IsDevelopment() && antiforgeryException is null)
            {
                problem.Detail = exception.Message;
            }

            problem.Extensions["errorCode"] = errorCode;
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = statusCode;

            await _problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });

            return true;
        }

        private static (int StatusCode, string ErrorCode, string Title) MapException(Exception exception)
        {
            if (FindException<AntiforgeryValidationException>(exception) is not null)
            {
                return (
                    StatusCodes.Status400BadRequest,
                    "security.csrf_validation_failed",
                    "CSRF validation failed");
            }

            if (exception is DbUpdateConcurrencyException)
            {
                return (
                    StatusCodes.Status409Conflict,
                    "db.concurrency_conflict",
                    "Concurrency conflict");
            }

            if (exception is UniqueConstraintException)
            {
                return (
                    StatusCodes.Status409Conflict,
                    "db.unique_constraint_violation",
                    "Unique constraint violation");
            }

            if (exception is ReferenceConstraintException)
            {
                return (
                    StatusCodes.Status409Conflict,
                    "db.foreign_key_constraint_violation",
                    "Foreign key constraint violation");
            }

            if (exception is CannotInsertNullException)
            {
                return (
                    StatusCodes.Status400BadRequest,
                    "db.not_null_constraint_violation",
                    "Not-null constraint violation");
            }

            var postgresException = FindPostgresException(exception);
            if (postgresException is not null && postgresException.SqlState == "23514")
            {
                return (
                    StatusCodes.Status400BadRequest,
                    "db.check_constraint_violation",
                    "Check constraint violation");
            }

            if (exception is DbUpdateException)
            {
                return (
                    StatusCodes.Status500InternalServerError,
                    "db.update_failed",
                    "Database update failed");
            }

            return (
                StatusCodes.Status500InternalServerError,
                "internal_server_error",
                "Unexpected error");
        }

        private static PostgresException? FindPostgresException(Exception exception)
        {
            return FindException<PostgresException>(exception);
        }

        private static TException? FindException<TException>(Exception exception)
            where TException : Exception
        {
            Exception? current = exception;
            while (current is not null)
            {
                if (current is TException typedException)
                    return typedException;

                current = current.InnerException;
            }

            return null;
        }
    }
}
