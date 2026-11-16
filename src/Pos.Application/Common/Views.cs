namespace Pos.Application.Common;

public sealed record StudentView(Guid Id, string FullName);

public sealed record CriterionView(Guid Id, string Name, decimal MaxScore, decimal Weight);

public sealed record EvaluationView(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid CriterionId,
    string CriterionName,
    decimal Score,
    decimal? MaxScore,
    DateOnly GradedOn,
    string? Comment);

public sealed record CriterionScoreView(Guid CriterionId, string CriterionName, decimal? Score, decimal MaxScore, decimal Weight);

public sealed record FinalGradeView(
    Guid StudentId,
    string StudentName,
    decimal Value,
    string PolicyName,
    int GradedCount,
    IReadOnlyList<CriterionScoreView> Breakdown);
