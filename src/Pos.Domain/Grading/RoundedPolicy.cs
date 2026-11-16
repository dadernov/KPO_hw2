namespace Pos.Domain.Grading;

public sealed class RoundedPolicy : IFinalGradePolicy
{
    private readonly IFinalGradePolicy _inner;
    private readonly int _digits;

    public RoundedPolicy(IFinalGradePolicy inner, int digits)
    {
        if (digits is < 0 or > 4)
            throw new ArgumentOutOfRangeException(nameof(digits), "Округлять можно до 0..4 знаков после запятой");
        _inner = inner;
        _digits = digits;
    }

    public string Name => _digits == 0
        ? $"{_inner.Name}; округление до целого"
        : $"{_inner.Name}; округление до {_digits} зн. после запятой";

    public decimal Compute(IReadOnlyList<CriterionResult> results) =>
        decimal.Round(_inner.Compute(results), _digits, MidpointRounding.AwayFromZero);
}
