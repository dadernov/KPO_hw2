using System.Globalization;
using Pos.Application.Common;
using Pos.Domain.Common;

namespace Pos.ConsoleApp;

public sealed class InputEndedException : Exception;

public sealed class ConsoleIo
{
    public string Ask(string prompt)
    {
        Console.Write($"{prompt}: ");
        var line = Console.ReadLine() ?? throw new InputEndedException();
        return line.Trim();
    }

    public string? AskOptional(string prompt)
    {
        var value = Ask($"{prompt} (Enter — пропустить)");
        return value.Length == 0 ? null : value;
    }

    public decimal AskDecimal(string prompt)
    {
        while (true)
        {
            var raw = Ask(prompt).Replace(',', '.');
            if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                return value;
            Error("Нужно число, например 7 или 8,5");
        }
    }

    public int Menu(string title, IReadOnlyList<string> items)
    {
        Title(title);
        for (var i = 0; i < items.Count; i++)
            Console.WriteLine($"  {i + 1}. {items[i]}");
        Console.WriteLine("  0. Назад");
        return AskIndex(items.Count);
    }

    public T? Pick<T>(string title, IReadOnlyList<T> items, Func<T, string> label) where T : class
    {
        if (items.Count == 0)
        {
            Info("Список пуст");
            return null;
        }
        Console.WriteLine($"{title}:");
        for (var i = 0; i < items.Count; i++)
            Console.WriteLine($"  {i + 1}. {label(items[i])}");
        Console.WriteLine("  0. Отмена");
        var index = AskIndex(items.Count);
        return index == 0 ? null : items[index - 1];
    }

    public void Safely(Action action)
    {
        try
        {
            action();
        }
        catch (DomainException e)
        {
            Error(e.Message);
        }
        catch (UseCaseException e)
        {
            Error(e.Message);
        }
        catch (InvalidDataException e)
        {
            Error(e.Message);
        }
        catch (IOException e)
        {
            Error($"Ошибка ввода-вывода: {e.Message}");
        }
        catch (UnauthorizedAccessException e)
        {
            Error($"Нет доступа: {e.Message}");
        }
    }

    public void Title(string text)
    {
        Console.WriteLine();
        Write($"== {text} ==", ConsoleColor.Cyan);
    }

    public void Line(string text = "") => Console.WriteLine(text);

    public void Info(string text) => Write(text, ConsoleColor.DarkGray);

    public void Success(string text) => Write(text, ConsoleColor.Green);

    public void Error(string text) => Write($"Ошибка: {text}", ConsoleColor.Red);

    public static string Num(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private int AskIndex(int max)
    {
        while (true)
        {
            if (int.TryParse(Ask("Выбор"), out var index) && index >= 0 && index <= max)
                return index;
            Error($"Введите число от 0 до {max}");
        }
    }

    private static void Write(string text, ConsoleColor color)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = previous;
    }
}
