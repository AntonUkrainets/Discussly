using Discussly.Server.Data.Entities.Comments;
using Discussly.Server.SharedKernel.DTO;

namespace Discussly.Server.Data.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task AddCommentAsync(Comment comment, CancellationToken cancellationToken = default);

        Task<bool> IsCommentExistsAsync(Guid id, CancellationToken cancellationToken = default);

        Task<CommentDto[]> GetLatestCommentsAsync(Guid[] rootCommentsIds, CancellationToken cancellationToken = default);

        Task<Comment[]> GetAllCommentsAsync(CancellationToken cancellationToken = default);

        Task UpdateCommentAttachmentAsync(UpdateCommentAttachment comment, CancellationToken cancellationToken = default);

        Task<CommentDto> GetCommentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}