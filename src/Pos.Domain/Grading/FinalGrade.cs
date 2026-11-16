using Pos.Domain.Common;
using Pos.Domain.Students;

namespace Pos.Domain.Grading;

public sealed record FinalGrade
{
    public FinalGrade(StudentId studentId, decimal value, string policyName, IReadOnlyList<CriterionResult> results)
    {
        if (value < 0 || value > GradeScale.Max)
            throw new DomainException($"Итог {value} вне шкалы 0..{GradeScale.Max}");
        StudentId = studentId;
        Value = value;
        PolicyName = policyName;
        Results = results;
    }

    public StudentId StudentId { get; }
    public decimal Value { get; }
    public string PolicyName { get; }
    public IReadOnlyList<CriterionResult> Results { get; }

    public int GradedCount => Results.Count(r => r.IsGraded);
}
