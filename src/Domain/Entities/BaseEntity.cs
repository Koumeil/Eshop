namespace Domain.Entities;

public abstract class BaseEntity : IEquatable<BaseEntity>
{
    public Guid Id { get; protected init; } = Guid.NewGuid();

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
    }

    public void MarkAsRestored()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        IsActive = true;
        DeletedAt = null;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        if (IsActive || IsDeleted) return;
        IsActive = true;
        MarkAsUpdated();
    }

    public bool Equals(BaseEntity? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Id == Guid.Empty || other.Id == Guid.Empty)
            return false;

        return Id == other.Id && GetType() == other.GetType();
    }

    public override bool Equals(object? obj) => Equals(obj as BaseEntity);
    public override int GetHashCode() => (GetType().ToString(), Id).GetHashCode();
}
