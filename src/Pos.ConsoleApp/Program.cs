using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Pos.ConsoleApp;
using Pos.ConsoleApp.Menus;
using Pos.Infrastructure;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var workDir = Environment.CurrentDirectory;
var options = new StorageOptions(Path.Combine(workDir, "pos-data"), Path.Combine(workDir, "export"));

using var provider = CompositionRoot.Build(options);

try
{
    if (args.Contains("--demo"))
        provider.GetRequiredService<DemoScenario>().Run();
    else
        provider.GetRequiredService<MainMenu>().Run();
}
catch (InputEndedException)
{
    Console.WriteLine();
}
