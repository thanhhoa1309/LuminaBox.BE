namespace LuminaBox.Application.Exceptions
{
    /// <summary>
    /// Base class for all application domain exceptions.
    /// Carries an HTTP status code so the GlobalExceptionMiddleware can respond correctly
    /// without any magic string lookups in Exception.Data.
    /// </summary>
    public class AppException : Exception
    {
        public int StatusCode { get; }

        /// <summary>
        /// Optional machine-usable failure code for <c>ApiResult.error.code</c>
        /// (e.g. <c>USER_NOT_FOUND</c>). When null, middleware falls back to the HTTP status string.
        /// </summary>
        public string? ErrorCode { get; }

        public AppException(int statusCode, string message, string? errorCode = null) : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public AppException(int statusCode, string message, Exception innerException, string? errorCode = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }

    // ─── Concrete typed exceptions ──────────────────────────────────────────────

    /// <summary>400 — Invalid request data or business rule violation.</summary>
    public class ExceptionBadRequest : AppException
    {
        public ExceptionBadRequest(string message = "Invalid request data.", string? errorCode = null)
            : base(400, message, errorCode) { }
    }

    /// <summary>401 — User is not authenticated.</summary>
    public class ExceptionUnauthorized : AppException
    {
        public ExceptionUnauthorized(string message = "Unauthorized.", string? errorCode = null)
            : base(401, message, errorCode) { }
    }

    /// <summary>403 — User is authenticated but does not have permission.</summary>
    public class ExceptionForbidden : AppException
    {
        public ExceptionForbidden(string message = "Access denied.", string? errorCode = null)
            : base(403, message, errorCode) { }
    }

    /// <summary>404 — Requested resource does not exist.</summary>
    public class ExceptionNotFound : AppException
    {
        public ExceptionNotFound(string message = "Resource not found.", string? errorCode = null)
            : base(404, message, errorCode) { }
    }

    /// <summary>409 — Duplicate or conflicting data.</summary>
    public class ExceptionConflict : AppException
    {
        public ExceptionConflict(string message = "A conflict occurred.", string? errorCode = null)
            : base(409, message, errorCode) { }
    }

    /// <summary>410 — Resource or API surface permanently removed.</summary>
    public class ExceptionGone : AppException
    {
        public ExceptionGone(string message = "This resource is no longer available.", string? errorCode = null)
            : base(410, message, errorCode) { }
    }

    /// <summary>500 — Unexpected system error.</summary>
    public class ExceptionInternalServerError : AppException
    {
        public ExceptionInternalServerError(string message = "An internal server error occurred.", string? errorCode = null)
            : base(500, message, errorCode) { }

        public ExceptionInternalServerError(string message, Exception inner, string? errorCode = null)
            : base(500, message, inner, errorCode) { }
    }

    /// <summary>502 — An external service failed.</summary>
    public class ExceptionBadGateway : AppException
    {
        public ExceptionBadGateway(string message = "An external service failed.", string? errorCode = null)
            : base(502, message, errorCode) { }
    }
}
