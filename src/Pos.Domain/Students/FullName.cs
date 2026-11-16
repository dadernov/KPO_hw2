using Pos.Domain.Common;

namespace Pos.Domain.Students;

public sealed record FullName
{
    public const int MaxLength = 120;

    public FullName(string? value)
    {
        var normalized = Text.Normalize(value);
        if (normalized.Length == 0)
            throw new DomainException("ФИО студента не может быть пустым");
        if (normalized.Length > MaxLength)
            throw new DomainException($"ФИО длиннее {MaxLength} символов");
        Value = normalized;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
