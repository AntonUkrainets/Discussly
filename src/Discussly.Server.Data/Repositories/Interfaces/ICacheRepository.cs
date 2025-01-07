namespace Discussly.Server.Data.Repositories.Interfaces
{
    public interface ICacheRepository
    {
        Task<string?> GetStringAsync(string key, CancellationToken cancellationToken);

        Task AddOrUpdateAsync(string key, string value, CancellationToken cancellationToken);
    }
}