using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Exceptions;
using LuminaBox.Application.Utils;
using Npgsql;

namespace LuminaBox.Api.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            catch (AppException ex)
            {
                if (ex.StatusCode >= 500)
                    _logger.LogError(ex, "Application error: {Message}", ex.Message);

                await WriteErrorAsync(context, ex.StatusCode, ex.ErrorCode ?? ex.StatusCode.ToString(), ex.Message);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                _logger.LogWarning(ex, "Unique constraint violation");
                await WriteErrorAsync(context, StatusCodes.Status409Conflict, "409", "The request conflicts with existing data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "500", "An internal server error occurred.");
            }
        }

        private static Task WriteErrorAsync(HttpContext context, int statusCode, string code, string message)
        {
            if (context.Response.HasStarted)
                return Task.CompletedTask;

            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsJsonAsync(ApiResult.Failure(code, message));
        }
    }
}
