using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

public sealed class Rubric
{
    private readonly List<Criterion> _criteria = [];

    public IReadOnlyList<Criterion> Criteria => _criteria.AsReadOnly();

    public decimal TotalWeight => _criteria.Sum(c => c.Weight.Value);

    public static Rubric Restore(IEnumerable<(CriterionId Id, CriterionName Name, MaxScore MaxScore, Weight Weight)> criteria)
    {
        var rubric = new Rubric();
        foreach (var c in criteria)
            rubric.Add(c.Id, c.Name, c.MaxScore, c.Weight);
        return rubric;
    }

    public Criterion AddCriterion(CriterionName name, MaxScore maxScore, Weight weight) =>
        Add(CriterionId.New(), name, maxScore, weight);

    public bool Contains(CriterionId id) => _criteria.Any(c => c.Id == id);

    public Criterion? Find(CriterionId id) => _criteria.FirstOrDefault(c => c.Id == id);

    public Criterion Get(CriterionId id) =>
        Find(id) ?? throw new DomainException("Такого критерия нет в рубрикаторе: оценивать можно только по критериям рубрикатора");

    public void RenameCriterion(CriterionId id, CriterionName name)
    {
        var criterion = Get(id);
        EnsureNameIsFree(name, id);
        criterion.Rename(name);
    }

    public void ChangeWeight(CriterionId id, Weight weight) => Get(id).ChangeWeight(weight);

    public void RemoveCriterion(CriterionId id) => _criteria.Remove(Get(id));

    private Criterion Add(CriterionId id, CriterionName name, MaxScore maxScore, Weight weight)
    {
        if (Contains(id))
            throw new DomainException("Критерий с таким идентификатором уже есть в рубрикаторе");
        EnsureNameIsFree(name, id);
        var criterion = new Criterion(id, name, maxScore, weight);
        _criteria.Add(criterion);
        return criterion;
    }

    private void EnsureNameIsFree(CriterionName name, CriterionId owner)
    {
        if (_criteria.Any(c => c.Id != owner && c.Name.SameAs(name)))
            throw new DomainException($"Критерий «{name}» уже есть в рубрикаторе");
    }
}
