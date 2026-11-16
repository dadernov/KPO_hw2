using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Common;
using Pos.Application.Evaluations;
using Pos.Application.Reports;
using Pos.Application.Rubrics;
using Pos.Application.Students;
using Pos.Domain.Common;
using Pos.Infrastructure;

namespace Pos.ConsoleApp.Menus;

public sealed class DemoScenario(ConsoleIo io, StorageOptions options) : IMenuSection
{
    public string Title => "Демо: сквозной сценарий";

    public void Run()
    {
        var sandbox = Path.Combine(Path.GetTempPath(), "pos-demo-" + Guid.NewGuid().ToString("N"));
        io.Info($"Демо работает в отдельном хранилище ({sandbox}), ваши данные не трогаются");
        try
        {
            using var provider = CompositionRoot.Build(options with { DataDirectory = sandbox });
            io.Safely(() => Play(provider));
        }
        finally
        {
            if (Directory.Exists(sandbox))
                Directory.Delete(sandbox, recursive: true);
        }
    }

    private void Play(IServiceProvider provider)
    {
        var students = provider.GetRequiredService<StudentService>();
        var rubric = provider.GetRequiredService<RubricService>();
        var grading = provider.GetRequiredService<GradingService>();
        var reports = provider.GetRequiredService<ReportService>();
        var settings = provider.GetRequiredService<GradingSettings>();
        var printer = provider.GetRequiredService<GradesPrinter>();

        io.Title("1. Заводим студентов");
        var ivan = students.Register("Иванов Иван");
        var maria = students.Register("Петрова Мария");
        var oleg = students.Register("Сидоров Олег");
        foreach (var s in students.List())
            io.Line($"  {s.FullName}");

        io.Title("2. Собираем рубрикатор");
        var hw1 = rubric.AddCriterion("ДЗ-1", 10, 0.2m);
        var hw2 = rubric.AddCriterion("ДЗ-2", 10, 0.3m);
        var exam = rubric.AddCriterion("Экзамен", 100, 0.5m);
        foreach (var c in rubric.List())
            io.Line($"  {RubricMenu.Label(c)}");

        io.Title("3. Выставляем оценки");
        Show(grading.Grade(ivan.Id, hw1.Id, 9, null));
        var ivanHw2 = grading.Grade(ivan.Id, hw2.Id, 7.5m, "сдано с опозданием");
        Show(ivanHw2);
        Show(grading.Grade(ivan.Id, exam.Id, 81, null));
        Show(grading.Grade(maria.Id, hw1.Id, 10, null));
        Show(grading.Grade(maria.Id, hw2.Id, 10, "лучшее решение в группе"));
        Show(grading.Grade(maria.Id, exam.Id, 95, null));
        Show(grading.Grade(oleg.Id, hw1.Id, 6, null));

        io.Title("4. Пробуем нарушить правила домена");
        Attempt("12 баллов из 10 по ДЗ-2", () => grading.Grade(oleg.Id, hw2.Id, 12, null));
        Attempt("отрицательный балл", () => grading.Grade(oleg.Id, hw2.Id, -1, null));
        Attempt("оценка по критерию не из рубрикатора", () => grading.Grade(oleg.Id, Guid.NewGuid(), 5, null));
        Attempt("вторая оценка по тому же критерию", () => grading.Grade(ivan.Id, hw1.Id, 10, null));
        Attempt("исправление на 150 из 100 по экзамену", () =>
            grading.Correct(grading.ListForStudent(maria.Id).Single(e => e.CriterionId == exam.Id).Id, 150, null));
        Attempt("удаление критерия, по которому есть оценки", () => rubric.Remove(hw1.Id));

        io.Title("5. Исправляем оценку");
        Show(grading.Correct(ivanHw2.Id, 8, "пересдача"));

        io.Title("6. Итоги");
        settings.Mode = GradingMode.WeightedTotal;
        settings.RoundingDigits = null;
        printer.PrintTable(reports.CalculateAll());
        io.Line();
        settings.RoundingDigits = 0;
        printer.PrintTable(reports.CalculateAll());
        io.Line();
        settings.Mode = GradingMode.GradedOnly;
        settings.RoundingDigits = 1;
        printer.PrintTable(reports.CalculateAll());
        io.Line();
        printer.PrintDetailed(reports.Calculate(ivan.Id));

        io.Title("7. Экспорт итогов");
        settings.Mode = GradingMode.WeightedTotal;
        settings.RoundingDigits = 0;
        io.Success($"Файл: {reports.Export("demo-final-grades")}");
    }

    private void Show(EvaluationView e) => io.Line($"  {e.StudentName} → {EvaluationsMenu.Label(e)}");

    private void Attempt(string description, Action action)
    {
        try
        {
            action();
            io.Error($"{description}: модель это пропустила!");
        }
        catch (DomainException e)
        {
            io.Success($"  {description}: отклонено — {e.Message}");
        }
        catch (UseCaseException e)
        {
            io.Success($"  {description}: отклонено — {e.Message}");
        }
    }
}
