using Pos.Application.Reports;
using Pos.Application.Students;

namespace Pos.ConsoleApp.Menus;

public sealed class ReportsMenu(
    ConsoleIo io,
    StudentService students,
    ReportService reports,
    GradingSettings settings,
    GradesPrinter printer) : IMenuSection
{
    public string Title => "Итоги и экспорт";

    public void Run()
    {
        while (true)
        {
            io.Info($"Текущее правило: {reports.CurrentPolicy}");
            var choice = io.Menu(Title, ["Итог студента", "Итоги всех студентов", $"Экспорт итогов в {reports.ExportFormat}", "Правило расчёта итога"]);
            if (choice == 0)
                return;
            Action action = choice switch
            {
                1 => ShowOne,
                2 => ShowAll,
                3 => Export,
                _ => Configure
            };
            io.Safely(action);
        }
    }

    private void ShowOne()
    {
        var student = io.Pick("Студент", students.List(), s => s.FullName);
        if (student is null)
            return;
        printer.PrintDetailed(reports.Calculate(student.Id));
    }

    private void ShowAll() => printer.PrintTable(reports.CalculateAll());

    private void Export()
    {
        var name = io.AskOptional("Имя файла без расширения, по умолчанию final-grades") ?? "final-grades";
        var path = reports.Export(name);
        io.Success($"Итоги выгружены: {path}");
    }

    private void Configure()
    {
        var mode = io.Menu("Как считать итог", [
            "Взвешенная сумма по всему рубрикатору (нет оценки = 0)",
            "Текущий рейтинг (только выставленные оценки)"
        ]);
        if (mode == 0)
            return;
        var rounding = io.Menu("Округление", ["Без округления", "До целого (0,5 вверх)", "До десятых"]);
        if (rounding == 0)
            return;
        settings.Mode = mode == 1 ? GradingMode.WeightedTotal : GradingMode.GradedOnly;
        settings.RoundingDigits = rounding switch
        {
            1 => null,
            2 => 0,
            _ => 1
        };
        io.Success($"Правило: {reports.CurrentPolicy}");
    }
}
