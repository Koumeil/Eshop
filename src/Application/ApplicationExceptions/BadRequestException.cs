namespace Application.ApplicationExceptions;

// 7. BadRequestException.cs
public sealed class BadRequestException : ApplicationExceptionBase
{
    public override int StatusCode => 400;

    public BadRequestException(string message)
        : base(message)
    {
    }

    public BadRequestException(string parameter, string reason)
        : base($"Invalid parameter '{parameter}': {reason}")
    {
    }
}