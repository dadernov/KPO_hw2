using Pos.Application.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Application.Evaluations;

public sealed class GradingService(
    IStudentRepository students,
    IRubricRepository rubrics,
    IEvaluationRepository evaluations,
    EvaluationFactory factory,
    TimeProvider clock)
{
    public EvaluationView Grade(Guid studentId, Guid criterionId, decimal score, string? comment)
    {
        var student = LoadStudent(studentId);
        var rubric = rubrics.Get();
        var evaluation = factory.Create(
            student,
            rubric,
            new CriterionId(criterionId),
            new Score(score),
            Today(),
            comment,
            evaluations.GetByStudent(student.Id));
        evaluations.Save(evaluation);
        return ToView(evaluation, student, rubric);
    }

    public EvaluationView Correct(Guid evaluationId, decimal score, string? comment)
    {
        var evaluation = LoadEvaluation(evaluationId);
        var rubric = rubrics.Get();
        evaluation.Correct(new Score(score), rubric, Today(), comment);
        evaluations.Save(evaluation);
        return ToView(evaluation, LoadStudent(evaluation.StudentId.Value), rubric);
    }

    public void Cancel(Guid evaluationId)
    {
        var evaluation = LoadEvaluation(evaluationId);
        evaluations.Remove(evaluation.Id);
    }

    public IReadOnlyList<EvaluationView> ListForStudent(Guid studentId)
    {
        var student = LoadStudent(studentId);
        var rubric = rubrics.Get();
        return evaluations.GetByStudent(student.Id)
            .OrderBy(e => Position(rubric, e.CriterionId))
            .Select(e => ToView(e, student, rubric))
            .ToList();
    }

    public IReadOnlyList<EvaluationView> ListAll()
    {
        var rubric = rubrics.Get();
        var byId = students.GetAll().ToDictionary(s => s.Id);
        return evaluations.GetAll()
            .Where(e => byId.ContainsKey(e.StudentId))
            .Select(e => ToView(e, byId[e.StudentId], rubric))
            .OrderBy(v => v.StudentName, StringComparer.CurrentCulture)
            .ThenBy(v => Position(rubric, new CriterionId(v.CriterionId)))
            .ToList();
    }

    private DateOnly Today() => DateOnly.FromDateTime(clock.GetLocalNow().DateTime);

    private Student LoadStudent(Guid id) =>
        students.Find(new StudentId(id)) ?? throw new UseCaseException("Студент не найден");

    private Evaluation LoadEvaluation(Guid id) =>
        evaluations.Find(new EvaluationId(id)) ?? throw new UseCaseException("Оценка не найдена");

    private static int Position(Rubric rubric, CriterionId id)
    {
        for (var i = 0; i < rubric.Criteria.Count; i++)
            if (rubric.Criteria[i].Id == id)
                return i;
        return int.MaxValue;
    }

    private static EvaluationView ToView(Evaluation e, Student student, Rubric rubric)
    {
        var criterion = rubric.Find(e.CriterionId);
        return new EvaluationView(
            e.Id.Value,
            student.Id.Value,
            student.Name.Value,
            e.CriterionId.Value,
            criterion?.Name.Value ?? "<критерий удалён>",
            e.Score.Value,
            criterion?.MaxScore.Value,
            e.GradedOn,
            e.Comment);
    }
}
