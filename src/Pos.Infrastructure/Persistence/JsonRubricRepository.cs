using Pos.Domain.Rubrics;

namespace Pos.Infrastructure.Persistence;

public sealed class JsonRubricRepository(StorageOptions options) : IRubricRepository
{
    private readonly JsonFile<List<CriterionDto>> _file = new(Path.Combine(options.DataDirectory, "rubric.json"));

    public Rubric Get() =>
        Rubric.Restore(_file.Read().Select(d =>
            (new CriterionId(d.Id), new CriterionName(d.Name), new MaxScore(d.MaxScore), new Weight(d.Weight))));

    public void Save(Rubric rubric) =>
        _file.Write(rubric.Criteria
            .Select(c => new CriterionDto(c.Id.Value, c.Name.Value, c.MaxScore.Value, c.Weight.Value))
            .ToList());
}
