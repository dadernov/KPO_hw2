using Pos.Domain.Grading;

namespace Pos.Application.Reports;

public enum GradingMode
{
    WeightedTotal,
    GradedOnly
}

public sealed class GradingSettings
{
    private int? _roundingDigits = 0;

    public GradingMode Mode { get; set; } = GradingMode.WeightedTotal;

    public int? RoundingDigits
    {
        get => _roundingDigits;
        set
        {
            if (value is < 0 or > 4)
                throw new ArgumentOutOfRangeException(nameof(value), "Округлять можно до 0..4 знаков после запятой");
            _roundingDigits = value;
        }
    }

    public IFinalGradePolicy BuildPolicy()
    {
        IFinalGradePolicy policy = Mode switch
        {
            GradingMode.WeightedTotal => new WeightedTotalPolicy(),
            GradingMode.GradedOnly => new GradedOnlyPolicy(),
            _ => throw new ArgumentOutOfRangeException(nameof(Mode))
        };
        return RoundingDigits is { } digits ? new RoundedPolicy(policy, digits) : policy;
    }
}
