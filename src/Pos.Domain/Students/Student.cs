using Pos.Domain.Common;

namespace Pos.Domain.Students;

public sealed class Student : Entity<StudentId>
{
    public Student(StudentId id, FullName name) : base(id)
    {
        Name = name;
    }

    public FullName Name { get; private set; }

    public static Student Register(FullName name) => new(StudentId.New(), name);

    public void Rename(FullName name) => Name = name;
}
