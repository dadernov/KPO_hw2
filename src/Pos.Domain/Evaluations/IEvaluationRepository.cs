using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Evaluations;

public interface IEvaluationRepository
{
    IReadOnlyList<Evaluation> GetAll();
    IReadOnlyList<Evaluation> GetByStudent(StudentId studentId);
    Evaluation? Find(EvaluationId id);
    bool AnyForCriterion(CriterionId criterionId);
    void Save(Evaluation evaluation);
    void Remove(EvaluationId id);
    int RemoveByStudent(StudentId studentId);
}
