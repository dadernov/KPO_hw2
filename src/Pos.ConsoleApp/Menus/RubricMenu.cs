using Pos.Application.Common;
using Pos.Application.Rubrics;

namespace Pos.ConsoleApp.Menus;

public sealed class RubricMenu(ConsoleIo io, RubricService rubric) : IMenuSection
{
    public string Title => "Рубрикатор (критерии)";

    public void Run()
    {
        while (true)
        {
            var choice = io.Menu(Title, ["Список критериев", "Добавить критерий", "Переименовать", "Изменить вес", "Удалить"]);
            if (choice == 0)
                return;
            Action action = choice switch
            {
                1 => Show,
                2 => Add,
                3 => Rename,
                4 => ChangeWeight,
                _ => Remove
            };
            io.Safely(action);
        }
    }

    public static string Label(CriterionView c) =>
        $"{c.Name} — максимум {ConsoleIo.Num(c.MaxScore)}, вес {ConsoleIo.Num(c.Weight)}";

    private void Show()
    {
        var list = rubric.List();
        if (list.Count == 0)
        {
            io.Info("Рубрикатор пуст");
            return;
        }
        for (var i = 0; i < list.Count; i++)
            io.Line($"  {i + 1}. {Label(list[i])}");
        io.Info($"Суммарный вес: {ConsoleIo.Num(rubric.TotalWeight())} (веса относительные, итог нормируется к 10)");
    }

    private void Add()
    {
        var name = io.Ask("Название");
        var max = io.AskDecimal("Максимальный балл");
        var weight = io.AskDecimal("Вес в итоге");
        var criterion = rubric.AddCriterion(name, max, weight);
        io.Success($"Добавлен критерий: {Label(criterion)}");
    }

    private void Rename()
    {
        var criterion = io.Pick("Какой критерий переименовать", rubric.List(), Label);
        if (criterion is null)
            return;
        rubric.Rename(criterion.Id, io.Ask("Новое название"));
        io.Success("Готово");
    }

    private void ChangeWeight()
    {
        var criterion = io.Pick("Какому критерию изменить вес", rubric.List(), Label);
        if (criterion is null)
            return;
        rubric.ChangeWeight(criterion.Id, io.AskDecimal("Новый вес"));
        io.Success("Готово");
    }

    private void Remove()
    {
        var criterion = io.Pick("Какой критерий удалить", rubric.List(), Label);
        if (criterion is null)
            return;
        rubric.Remove(criterion.Id);
        io.Success($"Критерий «{criterion.Name}» удалён");
    }
}
