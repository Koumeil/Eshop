namespace Application.ApplicationExceptions;

/// <summary>
/// Base class for all application-level exceptions.
/// Provides a consistent structure for error handling across the application layer.
/// </summary>
public abstract class ApplicationExceptionBase : Exception
{
    /// <summary>
    /// Optional error code to identify the specific error type (useful for clients/UI).
    /// </summary>
    public virtual string Code => GetType().Name;

    /// <summary>
    /// The recommended HTTP status code associated with this exception.
    /// </summary>
    public abstract int StatusCode { get; }

    protected ApplicationExceptionBase(string message)
        : base(message) { }

    protected ApplicationExceptionBase(string message, Exception? innerException)
        : base(message, innerException) { }
}
