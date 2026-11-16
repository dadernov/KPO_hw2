namespace Pos.Domain.Grading;

public sealed class WeightedTotalPolicy : IFinalGradePolicy
{
    public string Name => "взвешенная сумма по всему рубрикатору (нет оценки = 0)";

    public decimal Compute(IReadOnlyList<CriterionResult> results)
    {
        var totalWeight = results.Sum(r => r.Criterion.Weight.Value);
        if (totalWeight == 0)
            return 0m;
        return results.Sum(r => r.Share * r.Criterion.Weight.Value) / totalWeight * GradeScale.Max;
    }
}
