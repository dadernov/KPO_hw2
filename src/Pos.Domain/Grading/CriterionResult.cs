using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Grading;

public sealed record CriterionResult(Criterion Criterion, Score? Score)
{
    public bool IsGraded => Score is not null;

    public decimal Share => Score is null ? 0m : Criterion.ShareOf(Score);
}
