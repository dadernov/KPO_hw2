namespace Pos.ConsoleApp.Menus;

public sealed class MainMenu(ConsoleIo io, IEnumerable<IMenuSection> sections)
{
    private readonly IReadOnlyList<IMenuSection> _sections = sections.ToList();

    public void Run()
    {
        io.Line("ПОС — Преподавательская Оценка Студентов");
        while (true)
        {
            var choice = io.Menu("Главное меню", _sections.Select(s => s.Title).ToList());
            if (choice == 0)
                return;
            _sections[choice - 1].Run();
        }
    }
}
