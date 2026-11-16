using System.Text.Encodings.Web;
using System.Text.Json;

namespace Pos.Infrastructure.Persistence;

internal sealed class JsonFile<T>(string path) where T : new()
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public T Read()
    {
        if (!File.Exists(path))
            return new T();
        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options) ?? new T();
        }
        catch (JsonException e)
        {
            throw new InvalidDataException($"Файл {path} повреждён: {e.Message}", e);
        }
    }

    public void Write(T value)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(value, Options));
        File.Move(temp, path, overwrite: true);
    }
}
