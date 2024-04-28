using Infrastructure.Database.Models.Dtos;
using Infrastructure.Database.Models.Entities;
using UniRepo.Extensions;

namespace RepositoryApi.Extensions;

public class ExtensionExamples
{
    public void GetDynamicProjectionExample()
    {
        // Example data
        var people = new List<Person>
        {
            new () { FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1980, 1, 1), Email = "john.doe@example.com" },
            new () { FirstName = "Jane", LastName = "Doe", DateOfBirth = new DateTime(1985, 5, 5), Email = "jane.doe@example.com" }
        }
        .AsQueryable();

        // Select columns
        var selectedColumns = new List<string> { "FirstName", "LastName" };

        // Get projection
        var projectedPeople = people.GetDynamicProjection<Person, PersonDto>(selectedColumns);

        // Output results to verify correctness
        foreach (var person in projectedPeople)
        {
            Console.WriteLine($"FirstName: {person.FirstName}, LastName: {person.LastName}");
        }
    }
}