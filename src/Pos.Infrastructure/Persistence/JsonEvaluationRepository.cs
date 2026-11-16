using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Infrastructure.Persistence;

public sealed class JsonEvaluationRepository(StorageOptions options) : IEvaluationRepository
{
    private readonly JsonFile<List<EvaluationDto>> _file = new(Path.Combine(options.DataDirectory, "evaluations.json"));

    public IReadOnlyList<Evaluation> GetAll() => _file.Read().Select(ToDomain).ToList();

    public IReadOnlyList<Evaluation> GetByStudent(StudentId studentId) =>
        _file.Read().Where(d => d.StudentId == studentId.Value).Select(ToDomain).ToList();

    public Evaluation? Find(EvaluationId id) =>
        _file.Read().Where(d => d.Id == id.Value).Select(ToDomain).FirstOrDefault();

    public bool AnyForCriterion(CriterionId criterionId) =>
        _file.Read().Any(d => d.CriterionId == criterionId.Value);

    public void Save(Evaluation evaluation)
    {
        var all = _file.Read();
        var dto = new EvaluationDto(
            evaluation.Id.Value,
            evaluation.StudentId.Value,
            evaluation.CriterionId.Value,
            evaluation.Score.Value,
            evaluation.GradedOn,
            evaluation.Comment);
        var index = all.FindIndex(d => d.Id == dto.Id);
        if (index >= 0)
            all[index] = dto;
        else
            all.Add(dto);
        _file.Write(all);
    }

    public void Remove(EvaluationId id)
    {
        var all = _file.Read();
        if (all.RemoveAll(d => d.Id == id.Value) > 0)
            _file.Write(all);
    }

    public int RemoveByStudent(StudentId studentId)
    {
        var all = _file.Read();
        var removed = all.RemoveAll(d => d.StudentId == studentId.Value);
        if (removed > 0)
            _file.Write(all);
        return removed;
    }

    private static Evaluation ToDomain(EvaluationDto d) =>
        Evaluation.Restore(
            new EvaluationId(d.Id),
            new StudentId(d.StudentId),
            new CriterionId(d.CriterionId),
            new Score(d.Score),
            d.GradedOn,
            d.Comment);
}
