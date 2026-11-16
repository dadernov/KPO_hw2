namespace Pos.Infrastructure.Persistence;

internal sealed record StudentDto(Guid Id, string FullName);

internal sealed record CriterionDto(Guid Id, string Name, decimal MaxScore, decimal Weight);

internal sealed record EvaluationDto(Guid Id, Guid StudentId, Guid CriterionId, decimal Score, DateOnly GradedOn, string? Comment);
