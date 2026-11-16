using Pos.Domain.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Grading;

public sealed class FinalGradeCalculator
{
    public FinalGrade Calculate(Student student, Rubric rubric, IEnumerable<Evaluation> evaluations, IFinalGradePolicy policy)
    {
        if (rubric.Criteria.Count == 0)
            throw new DomainException("Рубрикатор пуст — итог считать не из чего");

        var own = evaluations.Where(e => e.StudentId == student.Id).ToList();

        if (own.Any(e => !rubric.Contains(e.CriterionId)))
            throw new DomainException($"У студента {student.Name} есть оценка по критерию вне рубрикатора — итог был бы несогласован");

        if (own.GroupBy(e => e.CriterionId).Any(g => g.Count() > 1))
            throw new DomainException($"У студента {student.Name} несколько оценок по одному критерию — итог был бы несогласован");

        var results = rubric.Criteria
            .Select(c => new CriterionResult(c, own.FirstOrDefault(e => e.CriterionId == c.Id)?.Score))
            .ToList();

        return new FinalGrade(student.Id, policy.Compute(results), policy.Name, results);
    }
}
