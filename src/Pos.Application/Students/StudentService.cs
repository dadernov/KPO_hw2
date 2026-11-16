using Pos.Application.Common;
using Pos.Domain.Evaluations;
using Pos.Domain.Students;

namespace Pos.Application.Students;

public sealed class StudentService(IStudentRepository students, IEvaluationRepository evaluations)
{
    public StudentView Register(string fullName)
    {
        var student = Student.Register(new FullName(fullName));
        students.Save(student);
        return ToView(student);
    }

    public IReadOnlyList<StudentView> List() =>
        students.GetAll()
            .OrderBy(s => s.Name.Value, StringComparer.CurrentCulture)
            .Select(ToView)
            .ToList();

    public StudentView Rename(Guid studentId, string fullName)
    {
        var student = Load(studentId);
        student.Rename(new FullName(fullName));
        students.Save(student);
        return ToView(student);
    }

    public int Remove(Guid studentId)
    {
        var student = Load(studentId);
        var removedEvaluations = evaluations.RemoveByStudent(student.Id);
        students.Remove(student.Id);
        return removedEvaluations;
    }

    private Student Load(Guid id) =>
        students.Find(new StudentId(id)) ?? throw new UseCaseException("Студент не найден");

    private static StudentView ToView(Student s) => new(s.Id.Value, s.Name.Value);
}
