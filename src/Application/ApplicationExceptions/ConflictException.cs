namespace Application.ApplicationExceptions;

public sealed class ConflictException : ApplicationExceptionBase
{
    public override int StatusCode => 409;

    public ConflictException(string resource, object key)
        : base($"{resource} with id '{key}' already exists.")
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }
}
