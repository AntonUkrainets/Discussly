using Discussly.Server.Data.Domain.Interfaces;
using Discussly.Server.Data.Repositories;
using Discussly.Server.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Discussly.Server.Data.Domain
{
    public class DiscussionDataUnitOfWork(DiscussionDataContext userContext, IDistributedCache distributedCache)
        : IDiscussionDataUnitOfWork, IDisposable
    {
        private readonly DiscussionDataContext _userContext = userContext;
        private IUserRepository _userRepository = null!;
        private ICommentRepository _commentRepository = null!;
        private ICacheRepository _cacheRepository = null!;
        private ISessionRepository _sessionRepository = null!;

        private bool _disposed;

        public IUserRepository Users => _userRepository ??= new UserRepository(_userContext);

        public ICommentRepository Comments => _commentRepository ??= new CommentRepository(_userContext);

        public ICacheRepository Cache => _cacheRepository ??= new CacheRepository(distributedCache);

        public ISessionRepository Sessions => _sessionRepository ??= new SessionRepository(Cache);

        public async Task SaveAsync(CancellationToken cancellationToken)
        {
            await _userContext.SaveChangesAsync(cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _userContext.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}