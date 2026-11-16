using Pos.Domain.Common;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Evaluations;

public sealed class Evaluation : Entity<EvaluationId>
{
    public const int CommentMaxLength = 300;

    internal Evaluation(EvaluationId id, StudentId studentId, CriterionId criterionId, Score score, DateOnly gradedOn, string? comment)
        : base(id)
    {
        StudentId = studentId;
        CriterionId = criterionId;
        Score = score;
        GradedOn = gradedOn;
        Comment = comment;
    }

    public StudentId StudentId { get; }
    public CriterionId CriterionId { get; }
    public Score Score { get; private set; }
    public DateOnly GradedOn { get; private set; }
    public string? Comment { get; private set; }

    public static Evaluation Restore(EvaluationId id, StudentId studentId, CriterionId criterionId, Score score, DateOnly gradedOn, string? comment) =>
        new(id, studentId, criterionId, score, gradedOn, NormalizeComment(comment));

    public void Correct(Score newScore, Rubric rubric, DateOnly correctedOn, string? comment)
    {
        rubric.Get(CriterionId).EnsureAccepts(newScore);
        var normalized = NormalizeComment(comment);
        Score = newScore;
        GradedOn = correctedOn;
        Comment = normalized ?? Comment;
    }

    internal static string? NormalizeComment(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return null;
        var trimmed = comment.Trim();
        if (trimmed.Length > CommentMaxLength)
            throw new DomainException($"Комментарий длиннее {CommentMaxLength} символов");
        return trimmed;
    }
}
