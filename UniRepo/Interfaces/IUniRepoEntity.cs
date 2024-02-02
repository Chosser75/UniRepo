namespace UniRepo.Interfaces;

public interface IUniRepoEntity<TIdType>
{
    public TIdType Id { get; set; }
}