using Pos.Application.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;

namespace Pos.Application.Rubrics;

public sealed class RubricService(IRubricRepository rubrics, IEvaluationRepository evaluations)
{
    public CriterionView AddCriterion(string name, decimal maxScore, decimal weight)
    {
        var rubric = rubrics.Get();
        var criterion = rubric.AddCriterion(new CriterionName(name), new MaxScore(maxScore), new Weight(weight));
        rubrics.Save(rubric);
        return ToView(criterion);
    }

    public IReadOnlyList<CriterionView> List() => rubrics.Get().Criteria.Select(ToView).ToList();

    public decimal TotalWeight() => rubrics.Get().TotalWeight;

    public void Rename(Guid criterionId, string name)
    {
        var rubric = rubrics.Get();
        rubric.RenameCriterion(new CriterionId(criterionId), new CriterionName(name));
        rubrics.Save(rubric);
    }

    public void ChangeWeight(Guid criterionId, decimal weight)
    {
        var rubric = rubrics.Get();
        rubric.ChangeWeight(new CriterionId(criterionId), new Weight(weight));
        rubrics.Save(rubric);
    }

    public void Remove(Guid criterionId)
    {
        var id = new CriterionId(criterionId);
        if (evaluations.AnyForCriterion(id))
            throw new UseCaseException("По этому критерию уже выставлены оценки — сначала отмените их, иначе итоги станут несогласованными");
        var rubric = rubrics.Get();
        rubric.RemoveCriterion(id);
        rubrics.Save(rubric);
    }

    private static CriterionView ToView(Criterion c) => new(c.Id.Value, c.Name.Value, c.MaxScore.Value, c.Weight.Value);
}
