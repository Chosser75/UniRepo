using UniRepo.Interfaces;

namespace Infrastructure.Database.Models.Entities;

#nullable disable

public class Person : IUniRepoEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }
}