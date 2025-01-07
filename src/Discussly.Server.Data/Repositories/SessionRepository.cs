using Discussly.Server.Data.Repositories.Interfaces;

namespace Discussly.Server.Data.Repositories
{
    public class SessionRepository(ICacheRepository cacheRepository) : ISessionRepository
    {
        public async Task AddOrUpdateSessionAsync(string email, string code, CancellationToken cancellationToken)
        {
            await cacheRepository.AddOrUpdateAsync(email, code, cancellationToken);
        }

        public async Task<string> GetCurrencySessionAsync(string email, CancellationToken cancellationToken)
        {
            var currentSession = await cacheRepository.GetStringAsync(email, cancellationToken);

            return currentSession ?? string.Empty;
        }
    }
}