namespace UniversalRepositoryTests.Models.Entities;

#nullable disable

public class TestPerson
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }
}