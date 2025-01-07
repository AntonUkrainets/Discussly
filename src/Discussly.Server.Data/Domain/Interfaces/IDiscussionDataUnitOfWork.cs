using Discussly.Server.Data.Repositories.Interfaces;

namespace Discussly.Server.Data.Domain.Interfaces
{
    public interface IDiscussionDataUnitOfWork
    {
        IUserRepository Users { get; }

        ICommentRepository Comments { get; }

        ISessionRepository Sessions { get; }

        Task SaveAsync(CancellationToken cancellationToken = default);
    }
}