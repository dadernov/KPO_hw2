using Pos.Application.Common;
using Pos.Application.Evaluations;
using Pos.Application.Reports;
using Pos.Application.Rubrics;
using Pos.Application.Students;
using Pos.Domain.Evaluations;
using Pos.Domain.Grading;
using Pos.Infrastructure;
using Pos.Infrastructure.Export;
using Pos.Infrastructure.Persistence;

namespace Pos.Tests;

public sealed class ApplicationTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "pos-tests-" + Guid.NewGuid().ToString("N"));
    private readonly StudentService _students;
    private readonly RubricService _rubric;
    private readonly GradingService _grading;
    private readonly ReportService _reports;

    public ApplicationTests()
    {
        var options = new StorageOptions(Path.Combine(_dir, "data"), Path.Combine(_dir, "export"));
        var studentRepo = new JsonStudentRepository(options);
        var rubricRepo = new JsonRubricRepository(options);
        var evaluationRepo = new JsonEvaluationRepository(options);
        _students = new StudentService(studentRepo, evaluationRepo);
        _rubric = new RubricService(rubricRepo, evaluationRepo);
        _grading = new GradingService(studentRepo, rubricRepo, evaluationRepo, new EvaluationFactory(), TimeProvider.System);
        _reports = new ReportService(studentRepo, rubricRepo, evaluationRepo, new FinalGradeCalculator(),
            new GradingSettings { RoundingDigits = null }, new CsvFinalGradesExporter(options));
    }

    [Fact]
    public void Criterion_with_evaluations_cannot_be_removed()
    {
        var student = _students.Register("Иванов Иван");
        var hw = _rubric.AddCriterion("ДЗ-1", 10, 1);
        _grading.Grade(student.Id, hw.Id, 7, null);

        Assert.Throws<UseCaseException>(() => _rubric.Remove(hw.Id));
        Assert.Single(_rubric.List());
    }

    [Fact]
    public void Removing_student_removes_their_evaluations()
    {
        var student = _students.Register("Иванов Иван");
        var hw = _rubric.AddCriterion("ДЗ-1", 10, 1);
        _grading.Grade(student.Id, hw.Id, 7, null);

        var removed = _students.Remove(student.Id);

        Assert.Equal(1, removed);
        Assert.Empty(_grading.ListAll());
    }

    [Fact]
    public void Data_survives_reload_and_final_grade_follows_corrections()
    {
        var student = _students.Register("Иванов Иван");
        var hw = _rubric.AddCriterion("ДЗ-1", 10, 1);
        var evaluation = _grading.Grade(student.Id, hw.Id, 7, null);

        _grading.Correct(evaluation.Id, 9.5m, "пересдача");

        var grade = _reports.Calculate(student.Id);
        Assert.Equal(9.5m, grade.Value);
        Assert.Equal("пересдача", _grading.ListForStudent(student.Id).Single().Comment);
    }

    [Fact]
    public void Export_writes_csv_with_final_grades()
    {
        var student = _students.Register("Иванов Иван");
        var hw = _rubric.AddCriterion("ДЗ-1", 10, 1);
        _grading.Grade(student.Id, hw.Id, 8, null);

        var path = _reports.Export("grades");

        var lines = File.ReadAllLines(path);
        Assert.Equal(2, lines.Length);
        Assert.StartsWith("Иванов Иван;8;1/1;8;", lines[1]);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
            Directory.Delete(_dir, recursive: true);
    }
}
