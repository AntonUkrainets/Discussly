using Discussly.Server.Data.Entities.Comments;
using Discussly.Server.SharedKernel.DTO;

namespace Discussly.Server.Infrastructure.Services.Elastic.Interfaces
{
    public interface IElasticSearchService
    {
        Task AddCommentsAsync(Comment[] comments, CancellationToken cancellationToken);

        Task AddCommentAsync(Comment comment, CancellationToken cancellationToken = default);

        Task<Guid[]> GetRootCommentsAsync(
            int pageIndex,
            int pageSize,
            string? searchText,
            CommentSortDirectionDto? sortDirection,
            CommentSortByDto? sortBy,
            CancellationToken cancellationToken);

        Task<int> GetTotalCountAsync(CancellationToken cancellationToken);
    }
}