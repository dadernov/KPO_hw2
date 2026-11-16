using Pos.Domain.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Tests;

public class DomainTests
{
    private static readonly DateOnly Today = new(2026, 11, 16);
    private readonly EvaluationFactory _factory = new();

    [Fact]
    public void Factory_rejects_score_above_criterion_maximum()
    {
        var (student, rubric, hw) = Setup();

        var error = Assert.Throws<DomainException>(() =>
            _factory.Create(student, rubric, hw.Id, new Score(12), Today, null, []));

        Assert.Contains("выходит за пределы", error.Message);
    }

    [Fact]
    public void Factory_accepts_boundary_scores()
    {
        var (student, rubric, hw) = Setup();

        var zero = _factory.Create(student, rubric, hw.Id, new Score(0), Today, null, []);
        var max = _factory.Create(Student.Register(new FullName("Другой")), rubric, hw.Id, new Score(10), Today, null, []);

        Assert.Equal(0m, zero.Score.Value);
        Assert.Equal(10m, max.Score.Value);
    }

    [Fact]
    public void Factory_rejects_criterion_outside_rubric()
    {
        var (student, rubric, _) = Setup();

        Assert.Throws<DomainException>(() =>
            _factory.Create(student, rubric, CriterionId.New(), new Score(5), Today, null, []));
    }

    [Fact]
    public void Factory_rejects_second_evaluation_for_same_criterion()
    {
        var (student, rubric, hw) = Setup();
        var first = _factory.Create(student, rubric, hw.Id, new Score(5), Today, null, []);

        Assert.Throws<DomainException>(() =>
            _factory.Create(student, rubric, hw.Id, new Score(7), Today, null, [first]));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("1.005")]
    public void Score_rejects_invalid_values(string value)
    {
        var parsed = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Throws<DomainException>(() => new Score(parsed));
    }

    [Fact]
    public void Correct_out_of_range_keeps_previous_score()
    {
        var (student, rubric, hw) = Setup();
        var evaluation = _factory.Create(student, rubric, hw.Id, new Score(5), Today, "ok", []);

        Assert.Throws<DomainException>(() => evaluation.Correct(new Score(11), rubric, Today, null));

        Assert.Equal(5m, evaluation.Score.Value);
        Assert.Equal("ok", evaluation.Comment);
    }

    [Fact]
    public void Rubric_rejects_duplicate_criterion_name_case_insensitive()
    {
        var rubric = new Rubric();
        rubric.AddCriterion(new CriterionName("Экзамен"), new MaxScore(100), new Weight(1));

        Assert.Throws<DomainException>(() =>
            rubric.AddCriterion(new CriterionName("  экзамен "), new MaxScore(10), new Weight(1)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Criterion_requires_positive_max_and_weight(int value)
    {
        Assert.Throws<DomainException>(() => new MaxScore(value));
        Assert.Throws<DomainException>(() => new Weight(value));
    }

    [Fact]
    public void Student_name_cannot_be_blank()
    {
        Assert.Throws<DomainException>(() => new FullName("   "));
    }

    private static (Student Student, Rubric Rubric, Criterion Hw) Setup()
    {
        var rubric = new Rubric();
        var hw = rubric.AddCriterion(new CriterionName("ДЗ-1"), new MaxScore(10), new Weight(1));
        return (Student.Register(new FullName("Иванов Иван")), rubric, hw);
    }
}
