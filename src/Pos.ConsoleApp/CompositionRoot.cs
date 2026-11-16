using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Evaluations;
using Pos.Application.Reports;
using Pos.Application.Rubrics;
using Pos.Application.Students;
using Pos.ConsoleApp.Menus;
using Pos.Domain.Evaluations;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Infrastructure;
using Pos.Infrastructure.Export;
using Pos.Infrastructure.Persistence;

namespace Pos.ConsoleApp;

public static class CompositionRoot
{
    public static ServiceProvider Build(StorageOptions options)
    {
        var services = new ServiceCollection();

        services.AddSingleton(options);
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IStudentRepository, JsonStudentRepository>();
        services.AddSingleton<IRubricRepository, JsonRubricRepository>();
        services.AddSingleton<IEvaluationRepository, JsonEvaluationRepository>();
        services.AddSingleton<IFinalGradesExporter, CsvFinalGradesExporter>();

        services.AddSingleton<EvaluationFactory>();
        services.AddSingleton<FinalGradeCalculator>();

        services.AddSingleton<GradingSettings>();
        services.AddSingleton<StudentService>();
        services.AddSingleton<RubricService>();
        services.AddSingleton<GradingService>();
        services.AddSingleton<ReportService>();

        services.AddSingleton<ConsoleIo>();
        services.AddSingleton<GradesPrinter>();
        services.AddSingleton<DemoScenario>();
        services.AddSingleton<IMenuSection, StudentsMenu>();
        services.AddSingleton<IMenuSection, RubricMenu>();
        services.AddSingleton<IMenuSection, EvaluationsMenu>();
        services.AddSingleton<IMenuSection, ReportsMenu>();
        services.AddSingleton<IMenuSection>(sp => sp.GetRequiredService<DemoScenario>());
        services.AddSingleton<MainMenu>();

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }
}
