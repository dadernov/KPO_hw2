using Pos.Application.Common;

namespace Pos.ConsoleApp.Menus;

public sealed class GradesPrinter(ConsoleIo io)
{
    public void PrintDetailed(FinalGradeView grade)
    {
        io.Line($"{grade.StudentName}");
        foreach (var b in grade.Breakdown)
        {
            var score = b.Score is { } s ? ConsoleIo.Num(s) : "—";
            io.Line($"  {b.CriterionName,-24} {score,6} / {ConsoleIo.Num(b.MaxScore),-6} вес {ConsoleIo.Num(b.Weight)}");
        }
        io.Success($"  Итог: {ConsoleIo.Num(grade.Value)} из 10 (оценено {grade.GradedCount}/{grade.Breakdown.Count})");
        io.Info($"  Правило: {grade.PolicyName}");
    }

    public void PrintTable(IReadOnlyList<FinalGradeView> grades)
    {
        if (grades.Count == 0)
        {
            io.Info("Студентов нет");
            return;
        }
        var width = Math.Max(10, grades.Max(g => g.StudentName.Length));
        foreach (var g in grades)
            io.Line($"  {g.StudentName.PadRight(width)}  {ConsoleIo.Num(g.Value),6}   (оценено {g.GradedCount}/{g.Breakdown.Count})");
        io.Info($"  Правило: {grades[0].PolicyName}");
    }
}
