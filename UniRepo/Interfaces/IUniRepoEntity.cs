namespace UniRepo.Interfaces;

public partial interface IUniRepoEntity<TIdType>
{
    public TIdType Id { get; set; }
}