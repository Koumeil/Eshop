namespace Application.ApplicationExceptions;

public sealed class NotFoundException : ApplicationExceptionBase
{
    public override int StatusCode => 404;

    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with id '{key}' was not found.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }
}
