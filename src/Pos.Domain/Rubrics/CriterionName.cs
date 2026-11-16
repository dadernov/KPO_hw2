using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

public sealed record CriterionName
{
    public const int MaxLength = 60;

    public CriterionName(string? value)
    {
        var normalized = Text.Normalize(value);
        if (normalized.Length == 0)
            throw new DomainException("Название критерия не может быть пустым");
        if (normalized.Length > MaxLength)
            throw new DomainException($"Название критерия длиннее {MaxLength} символов");
        Value = normalized;
    }

    public string Value { get; }

    public bool SameAs(CriterionName other) =>
        string.Equals(Value, other.Value, StringComparison.CurrentCultureIgnoreCase);

    public override string ToString() => Value;
}
