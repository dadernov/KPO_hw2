using Pos.Application.Common;

namespace Pos.Application.Reports;

public interface IFinalGradesExporter
{
    string Format { get; }
    string Export(IReadOnlyList<FinalGradeView> grades, string fileName);
}
