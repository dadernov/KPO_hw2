using System.Globalization;
using Pos.Domain.Common;

namespace Pos.Domain.Evaluations;

public sealed record Score
{
    public Score(decimal value)
    {
        if (value < 0)
            throw new DomainException($"Балл не может быть отрицательным: {value.ToString(CultureInfo.InvariantCulture)}");
        if (decimal.Round(value, 2) != value)
            throw new DomainException("Балл указывается не точнее сотых");
        Value = value;
    }

    public decimal Value { get; }

    public override string ToString() => Value.ToString("0.##", CultureInfo.InvariantCulture);
}
