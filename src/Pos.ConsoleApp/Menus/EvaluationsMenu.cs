using Pos.Application.Common;
using Pos.Application.Evaluations;
using Pos.Application.Rubrics;
using Pos.Application.Students;

namespace Pos.ConsoleApp.Menus;

public sealed class EvaluationsMenu(ConsoleIo io, StudentService students, RubricService rubric, GradingService grading)
    : IMenuSection
{
    public string Title => "Оценки";

    public void Run()
    {
        while (true)
        {
            var choice = io.Menu(Title, ["Выставить оценку", "Оценки студента", "Все оценки", "Исправить оценку", "Отменить оценку"]);
            if (choice == 0)
                return;
            Action action = choice switch
            {
                1 => Grade,
                2 => ShowForStudent,
                3 => ShowAll,
                4 => Correct,
                _ => Cancel
            };
            io.Safely(action);
        }
    }

    public static string Label(EvaluationView e)
    {
        var max = e.MaxScore is { } m ? $"/{ConsoleIo.Num(m)}" : string.Empty;
        var comment = e.Comment is null ? string.Empty : $" — «{e.Comment}»";
        return $"{e.CriterionName}: {ConsoleIo.Num(e.Score)}{max} ({e.GradedOn:dd.MM.yyyy}){comment}";
    }

    private void Grade()
    {
        var student = PickStudent();
        if (student is null)
            return;
        var criterion = io.Pick("Критерий", rubric.List(), RubricMenu.Label);
        if (criterion is null)
            return;
        var score = io.AskDecimal($"Балл (0..{ConsoleIo.Num(criterion.MaxScore)})");
        var comment = io.AskOptional("Комментарий");
        var evaluation = grading.Grade(student.Id, criterion.Id, score, comment);
        io.Success($"{student.FullName} → {Label(evaluation)}");
    }

    private void ShowForStudent()
    {
        var student = PickStudent();
        if (student is null)
            return;
        Print(grading.ListForStudent(student.Id), withStudent: false);
    }

    private void ShowAll() => Print(grading.ListAll(), withStudent: true);

    private void Correct()
    {
        var evaluation = PickEvaluation("Какую оценку исправить");
        if (evaluation is null)
            return;
        var score = io.AskDecimal("Новый балл");
        var comment = io.AskOptional("Новый комментарий");
        var corrected = grading.Correct(evaluation.Id, score, comment);
        io.Success($"Исправлено: {Label(corrected)}");
    }

    private void Cancel()
    {
        var evaluation = PickEvaluation("Какую оценку отменить");
        if (evaluation is null)
            return;
        grading.Cancel(evaluation.Id);
        io.Success($"Оценка отменена: {Label(evaluation)}");
    }

    private StudentView? PickStudent() => io.Pick("Студент", students.List(), s => s.FullName);

    private EvaluationView? PickEvaluation(string title)
    {
        var student = PickStudent();
        return student is null ? null : io.Pick(title, grading.ListForStudent(student.Id), Label);
    }

    private void Print(IReadOnlyList<EvaluationView> evaluations, bool withStudent)
    {
        if (evaluations.Count == 0)
        {
            io.Info("Оценок нет");
            return;
        }
        foreach (var e in evaluations)
            io.Line(withStudent ? $"  {e.StudentName} | {Label(e)}" : $"  {Label(e)}");
    }
}
