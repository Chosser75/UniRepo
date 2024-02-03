using UniRepo.Interfaces;

namespace Infrastructure.Database.Models.Base;

public class HasId : IUniRepoEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();
}