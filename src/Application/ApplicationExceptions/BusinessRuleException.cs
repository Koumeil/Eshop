namespace Application.ApplicationExceptions;

// 5. BusinessRuleException.cs
public sealed class BusinessRuleException : ApplicationExceptionBase
{
    public override int StatusCode => 422; // Unprocessable Entity

    public BusinessRuleException(string rule, string details)
        : base($"Business rule violation: {rule}. {details}")
    {
    }

    public BusinessRuleException(string message)
        : base(message)
    {
    }
}
