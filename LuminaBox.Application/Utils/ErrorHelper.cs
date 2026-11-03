using LuminaBox.Application.Exceptions;

namespace LuminaBox.Application.Utils
{
    public static class ErrorHelper
    {
        public static Exception BadRequest(string message = "Bad request.", string? errorCode = null)
            => new ExceptionBadRequest(message, errorCode);

        public static Exception Unauthorized(string message = "Unauthorized.", string? errorCode = null)
            => new ExceptionUnauthorized(message, errorCode);

        public static Exception Forbidden(string message = "Forbidden.", string? errorCode = null)
            => new ExceptionForbidden(message, errorCode);

        public static Exception NotFound(string message = "Resource not found.", string? errorCode = null)
            => new ExceptionNotFound(message, errorCode);

        public static Exception Conflict(string message = "Resource conflict.", string? errorCode = null)
            => new ExceptionConflict(message, errorCode);

        public static Exception Gone(string message = "Resource is no longer available.", string? errorCode = null)
            => new ExceptionGone(message, errorCode);

        public static Exception Internal(string message = "Internal server error.", string? errorCode = null)
            => new ExceptionInternalServerError(message, errorCode);

    }
}
