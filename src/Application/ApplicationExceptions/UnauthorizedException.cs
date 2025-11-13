namespace Application.ApplicationExceptions;

public sealed class UnauthorizedException : ApplicationExceptionBase
{
    public override int StatusCode => 401;

    public UnauthorizedException()
        : base("Authentication required.")
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
