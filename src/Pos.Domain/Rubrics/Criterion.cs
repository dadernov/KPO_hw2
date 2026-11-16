using Pos.Domain.Common;
using Pos.Domain.Evaluations;

namespace Pos.Domain.Rubrics;

public sealed class Criterion : Entity<CriterionId>
{
    internal Criterion(CriterionId id, CriterionName name, MaxScore maxScore, Weight weight) : base(id)
    {
        Name = name;
        MaxScore = maxScore;
        Weight = weight;
    }

    public CriterionName Name { get; private set; }
    public MaxScore MaxScore { get; }
    public Weight Weight { get; private set; }

    public bool Accepts(Score score) => score.Value <= MaxScore.Value;

    public void EnsureAccepts(Score score)
    {
        if (!Accepts(score))
            throw new DomainException(
                $"Балл {score} выходит за пределы критерия «{Name}»: допустимо от 0 до {MaxScore}");
    }

    public decimal ShareOf(Score score)
    {
        EnsureAccepts(score);
        return score.Value / MaxScore.Value;
    }

    internal void Rename(CriterionName name) => Name = name;

    internal void ChangeWeight(Weight weight) => Weight = weight;
}
