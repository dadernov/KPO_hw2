using Pos.Domain.Common;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Evaluations;

public sealed class EvaluationFactory
{
    public Evaluation Create(
        Student student,
        Rubric rubric,
        CriterionId criterionId,
        Score score,
        DateOnly gradedOn,
        string? comment,
        IEnumerable<Evaluation> existingEvaluations)
    {
        var criterion = rubric.Get(criterionId);
        criterion.EnsureAccepts(score);

        if (existingEvaluations.Any(e => e.StudentId == student.Id && e.CriterionId == criterionId))
            throw new DomainException(
                $"У студента {student.Name} уже есть оценка по критерию «{criterion.Name}» — её можно исправить или отменить");

        return new Evaluation(EvaluationId.New(), student.Id, criterionId, score, gradedOn, Evaluation.NormalizeComment(comment));
    }
}
