using Pos.Application.Students;

namespace Pos.ConsoleApp.Menus;

public sealed class StudentsMenu(ConsoleIo io, StudentService students) : IMenuSection
{
    public string Title => "Студенты";

    public void Run()
    {
        while (true)
        {
            var choice = io.Menu(Title, ["Список", "Добавить", "Переименовать", "Удалить"]);
            if (choice == 0)
                return;
            Action action = choice switch
            {
                1 => Show,
                2 => Add,
                3 => Rename,
                _ => Remove
            };
            io.Safely(action);
        }
    }

    private void Show()
    {
        var list = students.List();
        if (list.Count == 0)
        {
            io.Info("Студентов пока нет");
            return;
        }
        for (var i = 0; i < list.Count; i++)
            io.Line($"  {i + 1}. {list[i].FullName}");
    }

    private void Add()
    {
        var student = students.Register(io.Ask("ФИО"));
        io.Success($"Добавлен студент: {student.FullName}");
    }

    private void Rename()
    {
        var student = io.Pick("Кого переименовать", students.List(), s => s.FullName);
        if (student is null)
            return;
        var renamed = students.Rename(student.Id, io.Ask("Новое ФИО"));
        io.Success($"Теперь: {renamed.FullName}");
    }

    private void Remove()
    {
        var student = io.Pick("Кого удалить", students.List(), s => s.FullName);
        if (student is null)
            return;
        var removed = students.Remove(student.Id);
        io.Success($"Удалён {student.FullName}, снято оценок: {removed}");
    }
}
