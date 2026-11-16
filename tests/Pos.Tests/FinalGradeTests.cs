using Pos.Domain.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Tests;

public class FinalGradeTests
{
    private static readonly DateOnly Today = new(2026, 11, 16);
    private readonly EvaluationFactory _factory = new();
    private readonly FinalGradeCalculator _calculator = new();
    private readonly Rubric _rubric = new();
    private readonly Criterion _hw;
    private readonly Criterion _exam;
    private readonly Student _student = Student.Register(new FullName("Иванов Иван"));

    public FinalGradeTests()
    {
        _hw = _rubric.AddCriterion(new CriterionName("ДЗ"), new MaxScore(10), new Weight(0.4m));
        _exam = _rubric.AddCriterion(new CriterionName("Экзамен"), new MaxScore(100), new Weight(0.6m));
    }

    [Fact]
    public void Weighted_total_counts_missing_criteria_as_zero()
    {
        var evaluations = new[] { Grade(_hw, 8) };

        var grade = _calculator.Calculate(_student, _rubric, evaluations, new WeightedTotalPolicy());

        Assert.Equal(3.2m, grade.Value);
        Assert.Equal(1, grade.GradedCount);
    }

    [Fact]
    public void Weighted_total_uses_weights_and_criterion_scale()
    {
        var evaluations = new[] { Grade(_hw, 8), Grade(_exam, 70) };

        var grade = _calculator.Calculate(_student, _rubric, evaluations, new WeightedTotalPolicy());

        Assert.Equal(7.4m, grade.Value);
    }

    [Fact]
    public void Graded_only_ignores_missing_criteria()
    {
        var evaluations = new[] { Grade(_hw, 8) };

        var grade = _calculator.Calculate(_student, _rubric, evaluations, new GradedOnlyPolicy());

        Assert.Equal(8m, grade.Value);
    }

    [Theory]
    [InlineData(75, 8)]
    [InlineData(74, 7)]
    public void Rounding_decorator_rounds_half_away_from_zero(int examScore, int expected)
    {
        var evaluations = new[] { Grade(_hw, 7.5m), Grade(_exam, examScore) };
        var plain = _calculator.Calculate(_student, _rubric, evaluations, new WeightedTotalPolicy());

        var rounded = _calculator.Calculate(_student, _rubric, evaluations, new RoundedPolicy(new WeightedTotalPolicy(), 0));

        Assert.Equal((decimal)expected, rounded.Value);
        Assert.NotEqual(plain.Value, rounded.Value);
    }

    [Fact]
    public void Evaluations_of_other_students_are_ignored()
    {
        var other = Student.Register(new FullName("Петрова Мария"));
        var foreign = _factory.Create(other, _rubric, _exam.Id, new Score(100), Today, null, []);

        var grade = _calculator.Calculate(_student, _rubric, [Grade(_hw, 10), foreign], new WeightedTotalPolicy());

        Assert.Equal(4m, grade.Value);
    }

    [Fact]
    public void Evaluation_for_removed_criterion_makes_final_grade_inconsistent()
    {
        var evaluations = new[] { Grade(_hw, 8), Grade(_exam, 70) };
        _rubric.RemoveCriterion(_exam.Id);

        Assert.Throws<DomainException>(() =>
            _calculator.Calculate(_student, _rubric, evaluations, new WeightedTotalPolicy()));
    }

    [Fact]
    public void Empty_rubric_has_no_final_grade()
    {
        Assert.Throws<DomainException>(() =>
            _calculator.Calculate(_student, new Rubric(), [], new WeightedTotalPolicy()));
    }

    private Evaluation Grade(Criterion criterion, decimal score) =>
        _factory.Create(_student, _rubric, criterion.Id, new Score(score), Today, null, []);
}
