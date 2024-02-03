using Infrastructure.Database.Models.Base;

namespace Infrastructure.Database.Models.Entities;

#nullable disable

public class Person : HasId
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }
}