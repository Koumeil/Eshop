namespace Application.ApplicationExceptions;

// 4. ForbiddenException.cs
public sealed class ForbiddenException : ApplicationExceptionBase
{
    public override int StatusCode => 403;

    public ForbiddenException()
        : base("You don't have permission to access this resource.")
    {
    }

    public ForbiddenException(string resource, string action)
        : base($"You don't have permission to {action} the {resource}.")
    {
    }
}
