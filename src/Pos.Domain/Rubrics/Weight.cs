using System.Globalization;
using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

public sealed record Weight
{
    public const decimal Limit = 100m;

    public Weight(decimal value)
    {
        if (value <= 0)
            throw new DomainException("Вес критерия должен быть больше нуля");
        if (value > Limit)
            throw new DomainException($"Вес критерия не может превышать {Limit}");
        Value = value;
    }

    public decimal Value { get; }

    public override string ToString() => Value.ToString("0.####", CultureInfo.InvariantCulture);
}
