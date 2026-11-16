namespace Pos.Domain.Students;

public interface IStudentRepository
{
    IReadOnlyList<Student> GetAll();
    Student? Find(StudentId id);
    void Save(Student student);
    void Remove(StudentId id);
}
