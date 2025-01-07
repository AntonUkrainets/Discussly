namespace Discussly.Server.Data.Repositories.Interfaces
{
    public interface ISessionRepository
    {
        Task<string> GetCurrencySessionAsync(string email, CancellationToken cancellationToken);

        Task AddOrUpdateSessionAsync(string email, string code, CancellationToken cancellationToken);
    }
}