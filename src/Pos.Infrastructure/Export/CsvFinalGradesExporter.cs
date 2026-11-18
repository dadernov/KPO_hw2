using System.Globalization;
using System.Text;
using Pos.Application.Common;
using Pos.Application.Reports;

namespace Pos.Infrastructure.Export;

public sealed class CsvFinalGradesExporter(StorageOptions options) : IFinalGradesExporter
{
    private const char Separator = ';';

    public string Format => "CSV";

    public string Export(IReadOnlyList<FinalGradeView> grades, string fileName)
    {
        Directory.CreateDirectory(options.ExportDirectory);
        var path = Path.GetFullPath(Path.Combine(options.ExportDirectory, fileName + ".csv"));

        var criteria = grades
            .SelectMany(g => g.Breakdown)
            .Select(b => (b.CriterionId, b.CriterionName, b.MaxScore))
            .Distinct()
            .ToList();

        var csv = new StringBuilder();
        var header = new List<string> { "Студент" };
        header.AddRange(criteria.Select(c => $"{c.CriterionName} (из {Number(c.MaxScore)})"));
        header.AddRange(["Оценено критериев", "Итог", "Правило расчёта"]);
        AppendRow(csv, header);

        foreach (var grade in grades)
        {
            var row = new List<string> { grade.StudentName };
            row.AddRange(criteria.Select(c =>
            {
                var score = grade.Breakdown.FirstOrDefault(b => b.CriterionId == c.CriterionId)?.Score;
                return score is null ? string.Empty : Number(score.Value);
            }));
            row.Add($"{grade.GradedCount}/{grade.Breakdown.Count}");
            row.Add(Number(grade.Value));
            row.Add(grade.PolicyName);
            AppendRow(csv, row);
        }

        File.WriteAllText(path, csv.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        return path;
    }

    private static void AppendRow(StringBuilder csv, IEnumerable<string> cells) =>
        csv.AppendLine(string.Join(Separator, cells.Select(Escape)));

    private static string Escape(string cell) =>
        cell.IndexOfAny([Separator, '"', '\n', '\r']) >= 0
            ? "\"" + cell.Replace("\"", "\"\"") + "\""
            : cell;

    private static string Number(decimal value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');
}
