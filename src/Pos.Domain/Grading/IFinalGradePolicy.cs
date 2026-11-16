namespace Pos.Domain.Grading;

public interface IFinalGradePolicy
{
    string Name { get; }
    decimal Compute(IReadOnlyList<CriterionResult> results);
}
