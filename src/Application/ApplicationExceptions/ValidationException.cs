namespace Application.ApplicationExceptions;

public sealed class ValidationException : ApplicationExceptionBase
{
    public override int StatusCode => 400;
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Validation failed. Please check the errors for details.")
    {
        Errors = errors;
    }

    public ValidationException(string property, string error)
        : base($"Validation failed for {property}: {error}")
    {
        Errors = new Dictionary<string, string[]> { [property] = new[] { error } };
    }
}
