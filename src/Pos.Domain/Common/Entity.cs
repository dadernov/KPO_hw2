namespace Pos.Domain.Common;

public abstract class Entity<TId> where TId : struct
{
    protected Entity(TId id) => Id = id;

    public TId Id { get; }

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && other.GetType() == GetType() && Id.Equals(other.Id);

    public override int GetHashCode() => Id.GetHashCode();
}
