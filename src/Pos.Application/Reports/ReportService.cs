using Pos.Application.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Application.Reports;

public sealed class ReportService(
    IStudentRepository students,
    IRubricRepository rubrics,
    IEvaluationRepository evaluations,
    FinalGradeCalculator calculator,
    GradingSettings settings,
    IFinalGradesExporter exporter)
{
    public string ExportFormat => exporter.Format;

    public string CurrentPolicy => settings.BuildPolicy().Name;

    public FinalGradeView Calculate(Guid studentId)
    {
        var student = students.Find(new StudentId(studentId)) ?? throw new UseCaseException("Студент не найден");
        var grade = calculator.Calculate(student, rubrics.Get(), evaluations.GetByStudent(student.Id), settings.BuildPolicy());
        return ToView(grade, student);
    }

    public IReadOnlyList<FinalGradeView> CalculateAll()
    {
        var rubric = rubrics.Get();
        var policy = settings.BuildPolicy();
        var all = evaluations.GetAll();
        return students.GetAll()
            .OrderBy(s => s.Name.Value, StringComparer.CurrentCulture)
            .Select(s => ToView(calculator.Calculate(s, rubric, all, policy), s))
            .ToList();
    }

    public string Export(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new UseCaseException("Некорректное имя файла");
        var grades = CalculateAll();
        if (grades.Count == 0)
            throw new UseCaseException("Нет студентов — выгружать нечего");
        return exporter.Export(grades, fileName.Trim());
    }

    private static FinalGradeView ToView(FinalGrade grade, Student student) =>
        new(
            student.Id.Value,
            student.Name.Value,
            grade.Value,
            grade.PolicyName,
            grade.GradedCount,
            grade.Results
                .Select(r => new CriterionScoreView(
                    r.Criterion.Id.Value,
                    r.Criterion.Name.Value,
                    r.Score?.Value,
                    r.Criterion.MaxScore.Value,
                    r.Criterion.Weight.Value))
                .ToList());
}
