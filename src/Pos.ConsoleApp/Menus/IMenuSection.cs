namespace Pos.ConsoleApp.Menus;

public interface IMenuSection
{
    string Title { get; }
    void Run();
}
