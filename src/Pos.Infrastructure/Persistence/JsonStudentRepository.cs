using Pos.Domain.Students;

namespace Pos.Infrastructure.Persistence;

public sealed class JsonStudentRepository(StorageOptions options) : IStudentRepository
{
    private readonly JsonFile<List<StudentDto>> _file = new(Path.Combine(options.DataDirectory, "students.json"));

    public IReadOnlyList<Student> GetAll() => _file.Read().Select(ToDomain).ToList();

    public Student? Find(StudentId id) => GetAll().FirstOrDefault(s => s.Id == id);

    public void Save(Student student)
    {
        var all = _file.Read();
        var dto = new StudentDto(student.Id.Value, student.Name.Value);
        var index = all.FindIndex(d => d.Id == dto.Id);
        if (index >= 0)
            all[index] = dto;
        else
            all.Add(dto);
        _file.Write(all);
    }

    public void Remove(StudentId id)
    {
        var all = _file.Read();
        if (all.RemoveAll(d => d.Id == id.Value) > 0)
            _file.Write(all);
    }

    private static Student ToDomain(StudentDto d) => new(new StudentId(d.Id), new FullName(d.FullName));
}
