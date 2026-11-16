namespace Pos.Domain.Grading;

public sealed class GradedOnlyPolicy : IFinalGradePolicy
{
    public string Name => "текущий рейтинг (только выставленные оценки)";

    public decimal Compute(IReadOnlyList<CriterionResult> results)
    {
        var graded = results.Where(r => r.IsGraded).ToList();
        var totalWeight = graded.Sum(r => r.Criterion.Weight.Value);
        if (totalWeight == 0)
            return 0m;
        return graded.Sum(r => r.Share * r.Criterion.Weight.Value) / totalWeight * GradeScale.Max;
    }
}
