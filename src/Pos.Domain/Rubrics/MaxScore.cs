using System.Globalization;
using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

public sealed record MaxScore
{
    public const decimal Limit = 1000m;

    public MaxScore(decimal value)
    {
        if (value <= 0)
            throw new DomainException("Максимальный балл критерия должен быть больше нуля");
        if (value > Limit)
            throw new DomainException($"Максимальный балл критерия не может превышать {Limit}");
        if (decimal.Round(value, 2) != value)
            throw new DomainException("Максимальный балл указывается не точнее сотых");
        Value = value;
    }

    public decimal Value { get; }

    public override string ToString() => Value.ToString("0.##", CultureInfo.InvariantCulture);
}
