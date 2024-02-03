namespace Infrastructure.Database.Models.Base;

public class HasId
{
    public Guid Id { get; set; } = Guid.NewGuid();
}