namespace Infrastructure.Database.Interfaces;

public interface IDbService
{
    Task PopulateDatabaseAsync();
}