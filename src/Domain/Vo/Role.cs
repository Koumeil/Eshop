using Domain.DomainExceptions;

namespace Domain.Vo;


public readonly record struct Role
{
    public string Value { get; }

    public Role(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new DomainValidationException("Role cannot be null or whitespace.");

        Value = roleName.Trim();
        
        if (Value.Length > 50)
            throw new DomainValidationException($"Role cannot exceed 50 characters; got {Value.Length}.");
    }

    public static readonly Role Admin = new("Admin");
    public static readonly Role User = new("User");
    public static readonly Role Manager = new("Manager");

    public bool IsAdmin => Equals(Admin);
    public bool IsInRole(params Role[] roles) => roles.Contains(this);
    public override string ToString() => Value;
}
